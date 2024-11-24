

namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;

public class AssignmentImplementation : IAssignment
{
    private readonly List<Assignment> assignments = DataSource.Assignments;

    // יצירת משימה חדשה
    public void Create(Assignment item)
    {
        if (assignments.Any(a => a.Id == item.Id))
            throw new Exception("An assignment with this ID already exists.");

        assignments.Add(item);
    }

    // מחיקת משימה לפי מזהה
    public void Delete(int id)
    {
        Assignment? assignment = assignments.FirstOrDefault(a => a.Id == id);
        if (assignment == null)
            throw new Exception("Assignment not found.");

        assignments.Remove(assignment);
    }

    // מחיקת כל המשימות
    public void DeleteAll()
    {
        assignments.Clear();
    }

    // קריאת (חיפוש) משימה לפי מזהה
    public Assignment? Read(int id)
    {
        return assignments.FirstOrDefault(a => a.Id == id);
    }

    // קריאת כל המשימות
    public List<Assignment> ReadAll()
    {
        return assignments;
    }

    // עדכון משימה קיימת
    public void Update(Assignment item)
    {
        // חיפוש משימה קיימת לפי מזהה
        Assignment? existingAssignment = assignments.FirstOrDefault(a => a.Id == item.Id);

        // אם המשימה לא נמצאה, נזרוק חריגה
        if (existingAssignment == null)
            throw new Exception("Assignment not found.");

        // יצירת אובייקט חדש של Assignment עם הערכים המעודכנים
        Assignment? updatedAssignment = existingAssignment with
        {
            CallId = item.CallId,
            VolunteerId = item.VolunteerId,
            EntryTime = item.EntryTime,
            ActualEndTime = item.ActualEndTime,
            EndType = item.EndType
        };

        // מחיקת המשימה הישנה
        assignments.Remove(existingAssignment);

        // הוספת המשימה המעודכנת
        assignments.Add(updatedAssignment);
    }
}

