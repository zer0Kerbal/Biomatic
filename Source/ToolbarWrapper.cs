#if false
//
//  ToolbarWrapper.cs
//
//  Original Author:
//       toadicus <>
//
//  Generalised:
//      TriggerAu
//
//  Copyright (c) 2013 toadicus
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

internal static class ToolbarDLL
{
    public static Boolean Loaded 
    {
        get 
        {
            Type ToolbarManager = AssemblyLoader.loadedAssemblies
            .Select(a => a.assembly.GetExportedTypes())
                .SelectMany(t => t)
                .FirstOrDefault(t => t.FullName == "Toolbar.ToolbarManager");

            if (ToolbarManager == null)
            {
                return false;
            }
            return true;
        } 
    }
}

/// <summary>
/// Wraps a Toolbar clickable button, after fetching it from a foreign assembly.
/// </summary>
internal class ToolbarButtonWrapper
{
    protected System.Type ToolbarManager;
    protected object TBManagerInstance;
    protected MethodInfo TBManagerAdd;
    protected System.Type IButton;
    protected object Button;
    protected PropertyInfo ButtonText;
    protected PropertyInfo ButtonTextColor;
    protected PropertyInfo ButtonTexturePath;
    protected PropertyInfo ButtonToolTip;
    protected PropertyInfo ButtonVisible;
    protected PropertyInfo ButtonVisibility;
    protected PropertyInfo ButtonEnalbed;
    protected EventInfo ButtonOnClick;
    protected System.Type ClickHandlerType;
    protected MethodInfo ButtonDestroy;
    protected System.Type GameScenesVisibilityType;

    /// <summary>
    /// The text displayed on the button. Set to null to hide text.
    /// </summary>
    /// <remarks>
    /// The text can be changed at any time to modify the button's appearance. Note that since this will also
    /// modify the button's size, this feature should be used sparingly, if at all.
    /// </remarks>
    /// <seealso cref="TexturePath"/>
    public string Text
    {
        get
        {
            return this.ButtonText.GetValue(this.Button, null) as String;
        }
        set
        {
            this.ButtonText.SetValue(this.Button, value, null);
        }
    }

    /// <summary>
    /// The color the button text is displayed with. Defaults to Color.white.
    /// </summary>
    /// <remarks>
    /// The text color can be changed at any time to modify the button's appearance.
    /// </remarks>
    public Color TextColor
    {
        get
        {
            return (Color)this.ButtonTextColor.GetValue(this.Button, null);
        }
        set
        {
            this.ButtonTextColor.SetValue(this.Button, value, null);
        }
    }

    /// <summary>
    /// The path of a texture file to display an icon on the button. Set to null to hide icon.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A texture path on a button will have precedence over text. That is, if both text and texture path
    /// have been set on a button, the button will show the texture, not the text.
    /// </para>
    /// <para>
    /// The texture size must not exceed 24x24 pixels.
    /// </para>
    /// <para>
    /// The texture path must be relative to the "GameData" directory, and must not specify a file name suffix.
    /// Valid example: MyAddon/Textures/icon_mybutton
    /// </para>
    /// <para>
    /// The texture path can be changed at any time to modify the button's appearance.
    /// </para>
    /// </remarks>
    /// <seealso cref="Text"/>
    public string TexturePath
    {
        get
        {
            return this.ButtonTexturePath.GetValue(this.Button, null) as string;
        }
        set
        {
            this.ButtonTexturePath.SetValue(this.Button, value, null);
        }
    }

    /// <summary>
    /// The button's tool tip text. Set to null if no tool tip is desired.
    /// </summary>
    /// <remarks>
    /// Tool Tip Text Should Always Use Headline Style Like This.
    /// </remarks>
    public string ToolTip
    {
        get
        {
            return this.ButtonToolTip.GetValue(this.Button, null) as string;
        }
        set
        {
            this.ButtonToolTip.SetValue(this.Button, value, null);
        }
    }

    /// <summary>
    /// Whether this button is currently visible or not. Can be used in addition to or as a replacement for <see cref="Visibility"/>.
    /// </summary>
    public bool Visible
    {
        get
        {
            return (bool)this.ButtonVisible.GetValue(this.Button, null);
        }
        set
        {
            this.ButtonVisible.SetValue(this.Button, value, null);
        }
    }

    /// <summary>
    /// Whether this button is currently enabled (clickable) or not. This will not affect the player's ability to
    /// position the button on their screen.
    /// </summary>
    public bool Enabled
    {
        get
        {
            return (bool)this.ButtonEnalbed.GetValue(this.Button, null);
        }
        set
        {
            this.ButtonEnalbed.SetValue(this.Button, value, null);
        }
    }

    private ToolbarButtonWrapper() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="VOID.ToolbarButtonWrapper"/> class.
    /// </summary>
    /// <param name="ns">Namespace, usually the plugin name.</param>
    /// <param name="id">Identifier, unique per namespace.</param>
    public ToolbarButtonWrapper(string ns, string id)
    {
        this.ToolbarManager = AssemblyLoader.loadedAssemblies
                .Select(a => a.assembly.GetExportedTypes())
                .SelectMany(t => t)
                .FirstOrDefault(t => t.FullName == "Toolbar.ToolbarManager");

        this.TBManagerInstance = this.ToolbarManager.GetProperty(
                        "Instance",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static
                )
                .GetValue(null, null);

        this.TBManagerAdd = this.ToolbarManager.GetMethod("add");

        this.IButton = AssemblyLoader.loadedAssemblies
                .Select(a => a.assembly.GetExportedTypes())
                .SelectMany(t => t)
                .FirstOrDefault(t => t.FullName == "Toolbar.IButton");

        this.Button = this.TBManagerAdd.Invoke(this.TBManagerInstance, new object[] { ns, id });

        this.ButtonText = this.IButton.GetProperty("Text");

        this.ButtonTextColor = this.IButton.GetProperty("TextColor");

        this.ButtonTexturePath = this.IButton.GetProperty("TexturePath");

        this.ButtonToolTip = this.IButton.GetProperty("ToolTip");

        this.ButtonVisible = this.IButton.GetProperty("Visible");

        this.ButtonVisibility = this.IButton.GetProperty("Visibility");

        this.ButtonEnalbed = this.IButton.GetProperty("Enabled");

        this.ButtonOnClick = this.IButton.GetEvent("OnClick");

        this.ClickHandlerType = this.ButtonOnClick.EventHandlerType;

        this.ButtonDestroy = this.IButton.GetMethod("Destroy");

        this.GameScenesVisibilityType = AssemblyLoader.loadedAssemblies
                .Select(a => a.assembly.GetExportedTypes())
                        .SelectMany(t => t)
                        .FirstOrDefault(t => t.FullName == "Toolbar.GameScenesVisibility");
    }

    /// <summary>
    /// Adds event handler to receive "on click" events.
    /// </summary>
    /// <example>
    /// <code>
    /// ToolbarButtonWrapper button = ...
    /// button.AddButtonClickHandler(
    ///     (e) =>
    ///     {
    ///             Debug.Log("button clicked, mouseButton: " + e.Mousebutton");
    ///     }
    /// );
    /// </code>
    /// </example>
    /// <param name="Handler">Delegate to handle "on click" events</param>
    public void AddButtonClickHandler(Action<object> Handler)
    {
        Delegate d = Delegate.CreateDelegate(this.ClickHandlerType, Handler.Target, Handler.Method);
        MethodInfo addHandler = this.ButtonOnClick.GetAddMethod();
        addHandler.Invoke(this.Button, new object[] { d });
    }

    /// <summary>
    /// Sets this button's visibility. Can be used in addition to or as a replacement for <see cref="Visible"/>.
    /// </summary>
    /// <param name="gameScenes">Array of GameScene objects in which the button should be visible.</param>
    public void SetButtonVisibility(params GameScenes[] gameScenes)
    {
        object GameScenesVisibilityObj = Activator.CreateInstance(this.GameScenesVisibilityType, gameScenes);
        this.ButtonVisibility.SetValue(this.Button, GameScenesVisibilityObj, null);
    }

    /// <summary>
    /// Permanently destroys this button so that it is no longer displayed.
    /// Should be used when a plugin is stopped to remove leftover buttons.
    /// </summary>
    public void Destroy()
    {
        this.ButtonDestroy.Invoke(this.Button, null);
    }
}




#endif