using System.ComponentModel.DataAnnotations;

namespace Core.Enums
{
    public enum ScriptTask
    {
        [Display(Name = "Quick execution")]
        QuickExecution,
        [Display(Name = "Intelligent versioning")]
        IntelligentVersioning,
        [Display(Name = "Clean up/remove file versions")]
        VersionCleanUp
    }
}
