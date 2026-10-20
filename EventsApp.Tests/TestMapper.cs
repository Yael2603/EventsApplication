using AutoMapper;                                           // ספריית ההמרה
using EventsApp.Application.Mapping;                        // פרופיל ההמרות האמיתי של האפליקציה

namespace EventsApp.Tests;                                  // מרחב השמות של הבדיקות

public static class TestMapper                              // עוזר משותף: Mapper אמיתי, לא Mock
{
    public static IMapper Create() =>                       // יוצר Mapper עם אותן הגדרות כמו בשרת
        new MapperConfiguration(c => c.AddProfile<MappingProfile>()).CreateMapper();   // כך הבדיקות בודקות גם את ההמרות
}
