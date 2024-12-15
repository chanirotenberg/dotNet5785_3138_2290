namespace Dal;
using DalApi;
using DO;
using System.Linq;
using System.Xml.Linq;

internal class CallImplementation : ICall
{
    /// <summary>
    /// Converts an XElement to a Call object.
    /// </summary>
    /// <param name="s">The XElement to convert.</param>
    /// <returns>A Call object.</returns>
    /// <exception cref="FormatException">Thrown if required fields are missing or invalid.</exception>
    static Call getCall(XElement s)
    {
        return new Call
        {
            Id = s.ToIntNullable("Id") ?? throw new FormatException("Invalid or missing Id"),
            CallType = s.ToEnumNullable<CallType>("CallType") ?? CallType.Transport,
            VerbalDescription = (string?)s.Element("VerbalDescription") ?? null,
            Address = (string?)s.Element("Address") ?? "",
            Latitude = s.ToDoubleNullable("Latitude") ?? 0,
            Longitude = s.ToDoubleNullable("Longitude") ?? 0,
            OpeningTime = s.ToDateTimeNullable("OpeningTime") ?? DateTime.Now,
            MaximumTime = s.ToDateTimeNullable("MaximumTime") ?? null
        };
    }

    /// <summary>
    /// Creates a new call.
    /// </summary>
    /// <param name="item">The call object to create.</param>
    /// <exception cref="DalAlreadyExistsException">Thrown if a call with the given ID already exists.</exception>
    public void Create(Call item)
    {
        XElement callsRootElem = XMLTools.LoadListFromXMLElement(Config.s_calls_xml);

        // Check for duplicate ID
        if (callsRootElem.Elements().Any(c => (int?)c.Element("Id") == item.Id))
            throw new DalAlreadyExistsException($"Call with ID={item.Id} already exists");

        int newId = Config.NextCallId;
        Call newCall = item with { Id = newId };

        // Create an XML element for the new call and add it to the root
        callsRootElem.Add(createCallElement(newCall));
        XMLTools.SaveListToXMLElement(callsRootElem, Config.s_calls_xml);
    }

    /// <summary>
    /// Reads a call by its ID.
    /// </summary>
    /// <param name="id">The ID of the call to retrieve.</param>
    /// <returns>The call object if found; otherwise, null.</returns>
    public Call? Read(int id)
    {
        XElement? callElem = XMLTools.LoadListFromXMLElement(Config.s_calls_xml)
            .Elements()
            .FirstOrDefault(c => (int?)c.Element("Id") == id);

        return callElem is null ? null : getCall(callElem);
    }

    /// <summary>
    /// Reads a call that matches the given filter.
    /// </summary>
    /// <param name="filter">A predicate to filter the calls.</param>
    /// <returns>The first call that matches the filter; otherwise, null.</returns>
    public Call? Read(Func<Call, bool> filter)
    {
        return XMLTools.LoadListFromXMLElement(Config.s_calls_xml)
            .Elements()
            .Select(c => getCall(c))
            .FirstOrDefault(filter);
    }

    /// <summary>
    /// Reads all calls, optionally filtered by a predicate.
    /// </summary>
    /// <param name="filter">An optional filter to apply.</param>
    /// <returns>An enumerable of calls.</returns>
    public IEnumerable<Call> ReadAll(Func<Call, bool>? filter = null)
    {
        var calls = XMLTools.LoadListFromXMLElement(Config.s_calls_xml)
            .Elements()
            .Select(c => getCall(c));

        return filter == null ? calls : calls.Where(filter);
    }

    /// <summary>
    /// Updates an existing call with new values.
    /// </summary>
    /// <param name="item">The updated call object.</param>
    /// <exception cref="DalDoesNotExistException">Thrown if no call with the given ID exists.</exception>
    public void Update(Call item)
    {
        XElement callsRootElem = XMLTools.LoadListFromXMLElement(Config.s_calls_xml);

        (callsRootElem.Elements().FirstOrDefault(st => (int?)st.Element("Id") == item.Id)
        ?? throw new DalDoesNotExistException($"Call with ID={item.Id} does Not exist"))
                .Remove();

        callsRootElem.Add(new XElement("Call", createCallElement(item)));

        XMLTools.SaveListToXMLElement(callsRootElem, Config.s_calls_xml);
    }

    /// <summary>
    /// Deletes a call by its ID.
    /// </summary>
    /// <param name="id">The ID of the call to delete.</param>
    /// <exception cref="DalDoesNotExistException">Thrown if no call with the given ID exists.</exception>
    public void Delete(int id)
    {
        XElement callsRootElem = XMLTools.LoadListFromXMLElement(Config.s_calls_xml);

        XElement? callElem = callsRootElem.Elements()
            .FirstOrDefault(c => (int?)c.Element("Id") == id)
            ?? throw new DalDoesNotExistException($"Call with ID={id} does not exist");

        callElem.Remove();
        XMLTools.SaveListToXMLElement(callsRootElem, Config.s_calls_xml);
    }

    /// <summary>
    /// Deletes all calls.
    /// </summary>
    public void DeleteAll()
    {
        XElement callsRootElem = new XElement("ArrayOfCall");
        XMLTools.SaveListToXMLElement(callsRootElem, Config.s_calls_xml);
    }

    /// <summary>
    /// Converts a Call object into an XElement.
    /// </summary>
    /// <param name="call">The call object to convert.</param>
    /// <returns>An XElement representing the call.</returns>
    private static XElement createCallElement(Call call)
    {
        return new XElement("Call",
            new XElement("Id", call.Id),
            new XElement("CallType", call.CallType),
            new XElement("VerbalDescription", call.VerbalDescription),
            new XElement("Address", call.Address),
            new XElement("Latitude", call.Latitude),
            new XElement("Longitude", call.Longitude),
            new XElement("OpeningTime", call.OpeningTime),
            new XElement("MaximumTime", call.MaximumTime));
    }
}
