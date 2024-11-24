
namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;

public class CallImplementation : ICall
{
    // קריאת קריאה לפי מזהה
    public Call? Read(int id)
    {
        return calls.FirstOrDefault(c => c.Id == id);
    }

    // גישה לרשימת הקריאות המקורית ב-DataSource
    private readonly List<Call> calls = DataSource.Calls;

    // יצירת קריאה חדשה
    public void Create(Call item)
    {
        if (this.Read(item.Id) is not null)
            throw new Exception("An call with this ID already exists.");

        calls.Add(item);
    }

    // מחיקת קריאה לפי מזהה
    public void Delete(int id)
    {
        Call? call = calls.FirstOrDefault(c => c.Id == id);
        if (call == null)
            throw new Exception("Call not found.");

        calls.Remove(call);
    }

    // מחיקת כל הקריאות
    public void DeleteAll()
    {
        calls.Clear();
    }



    // קריאת כל הקריאות
    public List<Call> ReadAll()
    {
        return calls;
    }

    // עדכון קריאה קיימת
    public void Update(Call item)
    {
        Call? existingCall = calls.FirstOrDefault(c => c.Id == item.Id);
        if (existingCall == null)
            throw new Exception("Call not found.");

        // יצירת אובייקט חדש עם הערכים המעודכנים
        Call updatedCall = existingCall with
        {
            CallType = item.CallType,
            VerbalDescription = item.VerbalDescription,
            address = item.address,
            Latitude = item.Latitude,
            Longitude = item.Longitude,
            OpeningTime = item.OpeningTime,
            MaximumTime = item.MaximumTime
        };

        // מחיקת הקריאה הישנה והוספת הקריאה המעודכנת
        calls.Remove(existingCall);
        calls.Add(updatedCall);
    }
}
