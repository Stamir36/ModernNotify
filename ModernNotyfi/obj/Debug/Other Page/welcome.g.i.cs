﻿#pragma checksum "..\..\..\Other Page\welcome.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "2C53687BA664FA1330051859254E8AFEA4A149EB9B176E86C951DF463B4E6A66"
//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

using ModernNotyfi;
using ModernWpf;
using ModernWpf.Controls;
using ModernWpf.Controls.Primitives;
using ModernWpf.DesignTime;
using ModernWpf.Markup;
using ModernWpf.Media.Animation;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using XamlAnimatedGif;


namespace ModernNotyfi {
    
    
    /// <summary>
    /// welcome
    /// </summary>
    public partial class welcome : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 15 "..\..\..\Other Page\welcome.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image GifImage_Step_1;
        
        #line default
        #line hidden
        
        
        #line 16 "..\..\..\Other Page\welcome.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image GifImage_Step_2;
        
        #line default
        #line hidden
        
        
        #line 17 "..\..\..\Other Page\welcome.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TabControl WelcomTabs;
        
        #line default
        #line hidden
        
        
        #line 20 "..\..\..\Other Page\welcome.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Step_1;
        
        #line default
        #line hidden
        
        
        #line 31 "..\..\..\Other Page\welcome.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Step_2;
        
        #line default
        #line hidden
        
        
        #line 37 "..\..\..\Other Page\welcome.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Start_App;
        
        #line default
        #line hidden
        
        
        #line 47 "..\..\..\Other Page\welcome.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label Step_Text;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/ModernNotify;component/other%20page/welcome.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Other Page\welcome.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.GifImage_Step_1 = ((System.Windows.Controls.Image)(target));
            return;
            case 2:
            this.GifImage_Step_2 = ((System.Windows.Controls.Image)(target));
            return;
            case 3:
            this.WelcomTabs = ((System.Windows.Controls.TabControl)(target));
            return;
            case 4:
            this.Step_1 = ((System.Windows.Controls.Button)(target));
            
            #line 20 "..\..\..\Other Page\welcome.xaml"
            this.Step_1.Click += new System.Windows.RoutedEventHandler(this.Step_1_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.Step_2 = ((System.Windows.Controls.Button)(target));
            return;
            case 6:
            this.Start_App = ((System.Windows.Controls.Button)(target));
            
            #line 37 "..\..\..\Other Page\welcome.xaml"
            this.Start_App.Click += new System.Windows.RoutedEventHandler(this.Start_App_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.Step_Text = ((System.Windows.Controls.Label)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

