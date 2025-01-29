using System.ComponentModel.DataAnnotations;

namespace Core.Enums
{
    public enum ScriptTask
    {
        [Display(Name = "Quick execution")]
        QuickExecution,
    }

    public static class ScriptTaskMenuOptionsHelper
    {
        public static void CreateMenu()
        {
            var menuEnumType = typeof(ScriptTask);

            if (menuEnumType.IsEnum)
            {
                foreach (ScriptTask menuEnumValue in Enum.GetValues(menuEnumType))
                {
                    var fieldInfo = menuEnumType.GetField(menuEnumValue.ToString());
                    var menuEnumIndex = (int)Enum.Parse(typeof(ScriptTask), menuEnumValue.ToString());

                    if (null != fieldInfo)
                    {
                        var displayAttribute = Attribute.GetCustomAttribute(fieldInfo, typeof(DisplayAttribute));

                        if (null != displayAttribute)
                        {
                            Console.WriteLine($"{menuEnumIndex}: {((DisplayAttribute)displayAttribute).Name}");
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine($"{menuEnumType} is not an enum type.");
            }
        }
    }
}
