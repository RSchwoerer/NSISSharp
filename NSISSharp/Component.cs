using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NSISSharp
{
    /// <summary>
    /// Component class. Necessary for the Components page.
    /// </summary>
    public class Component
    {
        internal string Name { get; private set; }
        internal string Description { get; private set; }
        internal int[] InstallType { get; private set; }
        internal bool IsOptional { get; private set; }
        internal bool Expand { get; private set; }
        internal bool Bold { get; private set; }
        internal bool Hidden { get; private set; }
        internal bool IsGroup { get; set; }
        internal bool IsShortcutComponent { get; set; }
        internal List<Dir> Directories = new List<Dir>();
        internal List<Component> ComponentList = new List<Component>();
        internal List<object> ComponentSpecificActions = new List<object>();

        /// <summary>
        /// Creates a component object. This will represent a group which can contain other component objects.
        /// </summary>
        /// <param name="name">The name of the component. This will be visible to the user on the components page.</param>
        /// <param name="description">Description of the component. This will be displayed in the description textarea on the components page.</param>
        /// <param name="bold">Show component name in bold?</param>
        /// <param name="expand">Auto expand component?</param>
        /// <param name="componentobjects">Subcomponents.</param>
        public Component(string name, string description, bool bold, bool expand, params Component[] componentobjects)
        {
            /* Component group. */
            this.IsGroup = true;
            this.Name = name;
            this.Bold = bold;
            this.Hidden = false;
            this.Expand = expand;
            this.Description = description;
            this.IsShortcutComponent = false;

            if (componentobjects != null)
            {
                foreach (Component c in componentobjects)
                {
                    this.ComponentList.Add(c);
                }
            }
        }

        /// <summary>
        /// Creates a component object.
        /// </summary>
        /// <param name="name">The name of the component. This will be visible to the user in the components page.</param>
        /// <param name="description">Description of the component. This will be displayed in the description textarea on the components page.</param>
        /// <param name="optional">When set to true, the component in question will be unchecked by default.</param>
        /// <param name="bold">Show component name in bold?</param>
        /// <param name="hidden">When set to true, don't show the component on the component page.</param>
        /// <param name="installtype">Array of integers that will contain the install type indices of where the component in question belongs to.</param>
        /// <param name="actions">(Custom) Actions. Dir, RegValue, x64 and x86 objects allowed only, for now.</param>
        public Component(string name, string description, bool optional, bool bold, bool hidden, int[] installtype, params object[] actions)
        {
            /* Component. */
            this.IsGroup = false;
            this.Name = name;
            this.IsOptional = optional;
            this.Bold = bold;
            this.Hidden = hidden;
            this.Description = description;
            this.InstallType = installtype;
            this.IsShortcutComponent = false;

            if (actions != null)
            {
                for (int i = 0; i < actions.Length; i++)
                {
                    object Action = actions[i];

                    if (Action is Dir)
                    {
                        this.Directories.Add(Action as Dir);
                    }
                    else
                    {
                        if (Action is RegValue || Action is x64 || Action is x86)
                        {
                            this.ComponentSpecificActions.Add(Action);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Shortcut component class. Use this to represent the shortcuts on the components page.
    /// </summary>
    public class ShortcutComponent : Component
    {
        /// <summary>
        /// Creates a shortcut component object. This component will contain all shortcuts that can be created during install. It automatically creates two subcomponents: Desktop and Start Menu respectively.
        /// </summary>
        /// <param name="name">Name of the shortcuts 'group' component on the component page.</param>
        /// <param name="description_group">Description of the group.</param>
        /// <param name="description_desktopcomponent">Description of the Desktop component.</param>
        /// <param name="description_startmenucomponent">Description of the Start Menu component.</param>
        /// <param name="optional">When set to true: the component (the whole group) will be unchecked by default.</param>
        /// <param name="installtype">Array of integers that will contain the install type indices of where the component in question belongs to. This applies to the whole group.</param>
        public ShortcutComponent(string name, string description_group, string description_desktopcomponent, string description_startmenucomponent, bool optional, int[] installtype)
            : base(
                name, description_group, false, false,
                new Component("Desktop", description_desktopcomponent, optional, false, false, installtype, null),
                new Component("Start Menu", description_startmenucomponent, optional, false, false, installtype, null))
        {
            base.IsShortcutComponent = true;
            base.IsGroup = true;
        }

        internal Component ToComponent()
        {
            return (Component)base.MemberwiseClone();
        }
    }
}
