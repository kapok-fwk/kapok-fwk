namespace Kapok.Core;

// TODO: implement DisplayAttribute with translation
public enum FilterLayer
{
    /// <summary>
    /// Filter added from the system logic.
    ///
    /// Filter can not be changed from the user or application developer.
    /// </summary>
    System,

    /// <summary>
    /// Filter added from a license restriction.
    ///
    /// This permission can not be changed from the user or application developer.
    /// </summary>
    License,

    /// <summary>
    /// Filter added from the module developer.
    ///
    /// This permission can not be changed from the user.
    /// </summary>
    Module,

    /// <summary>
    /// Security permission filter.
    /// </summary>
    Permission,

    /// <summary>
    /// Filter added from the application code.
    /// </summary>
    Application,

    /// <summary>
    /// Filter added from the user in the GUI.
    /// </summary>
    User
}