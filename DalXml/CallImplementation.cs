
    namespace Dal;
using DalApi;
using DO;
using System.Linq;
using System.Xml.Linq;

internal class CallImplementation : ICall
{

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
    /// יוצרת קריאה חדשה.
    /// </summary>
    /// <param name="item">אובייקט הקריאה שנוצר.</param>
    /// <exception cref="DalAlreadyExistsException">נזרקת אם קריאה עם ה-ID הנתון כבר קיימת.</exception>
    
    public void Create(Call item)
    {
        XElement callsRootElem = XMLTools.LoadListFromXMLElement(Config.s_calls_xml);

        // בדיקה האם יש ID כפול
        if (callsRootElem.Elements().Any(c => (int?)c.Element("Id") == item.Id))
            throw new DalAlreadyExistsException($"Call with ID={item.Id} already exists");

        int newId = Config.NextCallId;
        Call newCall = item with { Id = newId };

        // יצירת אלמנט XML מתאים עבור ה-Call
        callsRootElem.Add(createCallElement(newCall));
        XMLTools.SaveListToXMLElement(callsRootElem, Config.s_calls_xml);
    }


    /// <summary>
    /// קורא (מאחזר) קריאה לפי ID.
    /// </summary>
    /// <param name="id">ה-ID של הקריאה שיש לאחזר.</param>
    /// <returns>אובייקט הקריאה אם נמצא; אחרת, null.</returns>
    public Call? Read(int id)
    {
        XElement? callElem = XMLTools.LoadListFromXMLElement(Config.s_calls_xml)
            .Elements()
            .FirstOrDefault(c => (int?)c.Element("Id") == id);

        return callElem is null ? null : getCall(callElem);
    }

    /// <summary>
    /// קורא קריאה לפי מסנן מותאם אישית.
    /// </summary>
    /// <param name="filter">מסנן מותאם אישית לקריאה.</param>
    /// <returns>האובייקט הראשון שעונה על המסנן, או null אם לא נמצא.</returns>
    public Call? Read(Func<Call, bool> filter)
    {
        return XMLTools.LoadListFromXMLElement(Config.s_calls_xml)
            .Elements()
            .Select(c => getCall(c))
            .FirstOrDefault(filter);
    }

    /// <summary>
    /// קורא את כל הקריאות עם מסנן אופציונלי.
    /// </summary>
    public IEnumerable<Call> ReadAll(Func<Call, bool>? filter = null)
    {
        var calls = XMLTools.LoadListFromXMLElement(Config.s_calls_xml)
            .Elements()
            .Select(c => getCall(c));

        return filter == null ? calls : calls.Where(filter);
    }

    /// <summary>
    /// מעדכן קריאה קיימת עם ערכים חדשים.
    /// </summary>

    public void Update(Call item)
    {
        XElement callsRootElem = XMLTools.LoadListFromXMLElement(Config.s_calls_xml);

        (callsRootElem.Elements().FirstOrDefault(st => (int?)st.Element("Id") == item.Id)
        ?? throw new DO.DalDoesNotExistException($"Call with ID={item.Id} does Not exist"))
                .Remove();

        callsRootElem.Add(new XElement("Call", createCallElement(item)));

        XMLTools.SaveListToXMLElement(callsRootElem, Config.s_calls_xml);
    }


    /// <summary>
    /// מוחקת קריאה לפי ID.
    /// </summary>
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
    /// מוחקת את כל הקריאות.
    /// </summary>
    public void DeleteAll()
    {
        XElement callsRootElem = new XElement("ArrayOfCall");
        XMLTools.SaveListToXMLElement(callsRootElem, Config.s_calls_xml);
    }


    /// <summary>
    /// ממיר אובייקט Call ל-XElement.
    /// </summary>
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