﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace SP.Studio.Files {
    using System;
    
    
    /// <summary>
    ///   一个强类型的资源类，用于查找本地化的字符串等。
    /// </summary>
    // 此类是由 StronglyTypedResourceBuilder
    // 类通过类似于 ResGen 或 Visual Studio 的工具自动生成的。
    // 若要添加或移除成员，请编辑 .ResX 文件，然后重新运行 ResGen
    // (以 /str 作为命令选项)，或重新生成 VS 项目。
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class WorkFlow {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal WorkFlow() {
        }
        
        /// <summary>
        ///   返回此类使用的缓存的 ResourceManager 实例。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("SP.Studio.Files.WorkFlow", typeof(WorkFlow).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   使用此强类型资源类，为所有资源查找
        ///   重写当前线程的 CurrentUICulture 属性。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   查找 System.Drawing.Bitmap 类型的本地化资源。
        /// </summary>
        internal static System.Drawing.Bitmap body {
            get {
                object obj = ResourceManager.GetObject("body", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   查找 System.Drawing.Bitmap 类型的本地化资源。
        /// </summary>
        internal static System.Drawing.Bitmap button {
            get {
                object obj = ResourceManager.GetObject("button", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   查找类似 &lt;!DOCTYPE html PUBLIC &quot;-//W3C//DTD XHTML 1.0 Transitional//EN&quot; &quot;http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd&quot;&gt;
        ///
        ///&lt;html xmlns=&quot;http://www.w3.org/1999/xhtml&quot;&gt;
        ///&lt;head&gt;
        ///    &lt;title&gt;&lt;/title&gt;
        ///&lt;/head&gt;
        ///&lt;body&gt;
        ///
        ///&lt;table&gt;
        ///    &lt;tr&gt;
        ///        &lt;th&gt;ID:&lt;/th&gt;
        ///        &lt;td&gt;&lt;input type=&quot;text&quot; name=&quot;ID&quot; readonly=&quot;readonly&quot; style=&quot;width:50px;&quot; /&gt;&lt;/td&gt;
        ///    &lt;/tr&gt;
        ///    &lt;tr&gt;
        ///        &lt;th&gt;名称:&lt;/th&gt;
        ///        &lt;td&gt;&lt;input type=&quot;text&quot; name=&quot;Name&quot; /&gt;&lt;/td&gt;
        ///    &lt;/tr&gt;
        ///    &lt;tr&gt;
        ///        &lt;th&gt;说明:&lt;/th&gt;
        ///        &lt;td&gt;&lt;textarea row [字符串的其余部分被截断]&quot;; 的本地化字符串。
        /// </summary>
        internal static string Event {
            get {
                return ResourceManager.GetString("Event", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找 System.Drawing.Bitmap 类型的本地化资源。
        /// </summary>
        internal static System.Drawing.Bitmap group {
            get {
                object obj = ResourceManager.GetObject("group", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   查找类似 $import(&quot;UI.Diag.js&quot;);
        ///$import(&quot;UI.FillForm.js&quot;);
        ///
        ///var isDrag = false;
        ///var GroupID = 0;    // 全局变量
        ///var t;
        ///
        ///window.addEvent(&quot;domready&quot;, function () {
        ///
        ///    var elem = $(&quot;canvas&quot;);
        ///    t = new UI.WorkFlow(elem);
        ///
        ///    // 工具栏
        ///    $$(&quot;.Bar &gt; ul &gt; li &gt; a&quot;).each(function (item) {
        ///        var drag = item.getNext();
        ///        if (drag == null || drag.get(&quot;tag&quot;) != &quot;div&quot;) return;
        ///        var position = item.getPosition();
        ///        drag.setStyles({
        ///            &quot;left&quot;: position.x,
        ///            &quot;top&quot;: posi [字符串的其余部分被截断]&quot;; 的本地化字符串。
        /// </summary>
        internal static string JScript {
            get {
                return ResourceManager.GetString("JScript", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 &lt;!DOCTYPE html PUBLIC &quot;-//W3C//DTD XHTML 1.0 Transitional//EN&quot; &quot;http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd&quot;&gt;
        ///
        ///&lt;html xmlns=&quot;http://www.w3.org/1999/xhtml&quot;&gt;
        ///&lt;head&gt;
        ///    &lt;title&gt;&lt;/title&gt;
        ///&lt;/head&gt;
        ///&lt;body&gt;
        ///&lt;div class=&quot;button&quot;&gt;
        ///    &lt;input type=&quot;hidden&quot; name=&quot;Genre&quot; value=&quot;line&quot; /&gt;
        ///    &lt;input type=&quot;hidden&quot; name=&quot;ID&quot; /&gt;
        ///    &lt;input type=&quot;button&quot; value=&quot;删 除&quot; id=&quot;btnDelete&quot; class=&quot;delete&quot; /&gt;
        ///
        ///    &lt;input type=&quot;hidden&quot; value=&quot;确 定&quot; id=&quot;btnSave&quot; /&gt;
        ///    &amp;nbsp;&amp;nbsp;
        ///    &lt;input type=&quot;hidden&quot; value=&quot;取 消 [字符串的其余部分被截断]&quot;; 的本地化字符串。
        /// </summary>
        internal static string Line {
            get {
                return ResourceManager.GetString("Line", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找 System.Drawing.Bitmap 类型的本地化资源。
        /// </summary>
        internal static System.Drawing.Bitmap loading {
            get {
                object obj = ResourceManager.GetObject("loading", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   查找类似 &lt;!DOCTYPE html PUBLIC &quot;-//W3C//DTD XHTML 1.0 Transitional//EN&quot; &quot;http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd&quot;&gt;
        ///
        ///&lt;html xmlns=&quot;http://www.w3.org/1999/xhtml&quot;&gt;
        ///&lt;head&gt;
        ///    &lt;title&gt;&lt;/title&gt;
        ///&lt;/head&gt;
        ///&lt;body&gt;
        ///
        ///&lt;table&gt;
        ///    &lt;tr&gt;
        ///        &lt;th&gt;ID:&lt;/th&gt;
        ///        &lt;td&gt;&lt;input type=&quot;text&quot; name=&quot;ID&quot; readonly=&quot;readonly&quot; style=&quot;width:50px;&quot; /&gt;&lt;/td&gt;
        ///    &lt;/tr&gt;
        ///    &lt;tr&gt;
        ///        &lt;th&gt;名称:&lt;/th&gt;
        ///        &lt;td&gt;&lt;input type=&quot;text&quot; name=&quot;Name&quot; /&gt;&lt;/td&gt;
        ///    &lt;/tr&gt;
        ///    &lt;tr&gt;
        ///        &lt;th&gt;说明:&lt;/th&gt;
        ///        &lt;td&gt;&lt;textarea row [字符串的其余部分被截断]&quot;; 的本地化字符串。
        /// </summary>
        internal static string Page {
            get {
                return ResourceManager.GetString("Page", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 &lt;!DOCTYPE html PUBLIC &quot;-//W3C//DTD XHTML 1.0 Transitional//EN&quot; &quot;http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd&quot;&gt;
        ///
        ///&lt;html xmlns=&quot;http://www.w3.org/1999/xhtml&quot;&gt;
        ///&lt;head&gt;
        ///    &lt;title&gt;&lt;/title&gt;
        ///&lt;/head&gt;
        ///&lt;body&gt;
        ///&lt;table&gt;
        ///    &lt;tr&gt;
        ///        &lt;th&gt;ID:&lt;/th&gt;
        ///        &lt;td&gt;&lt;input type=&quot;text&quot; name=&quot;ID&quot; readonly=&quot;readonly&quot; style=&quot;width:50px;&quot; /&gt;&lt;/td&gt;
        ///    &lt;/tr&gt;
        ///    &lt;tr&gt;
        ///        &lt;th&gt;返回值:&lt;/th&gt;
        ///        &lt;td&gt;&lt;input type=&quot;text&quot; name=&quot;Name&quot; /&gt;&lt;/td&gt;
        ///    &lt;/tr&gt;
        ///    &lt;tr&gt;
        ///        &lt;th&gt;说明:&lt;/th&gt;
        ///        &lt;td&gt;&lt;textarea rows [字符串的其余部分被截断]&quot;; 的本地化字符串。
        /// </summary>
        internal static string Result {
            get {
                return ResourceManager.GetString("Result", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 body { margin:0px; font-size:12px; background:#9595af;  }
        ///a{ text-decoration:none; }
        ///ul , li{ list-style-type:none; padding:0px; margin:0px; }
        ///.Container{ }
        ///.Container .ContainerLeft{ width:150px;   }
        ///
        ///.Toolbar{ background:#f8f8f8 url(workflow.axd?type=image&amp;src=tools.png) repeat-y -185px top; border-right:solid 2px #bfdbff; position:fixed; width:150px; height:100%; }
        ///.Toolbar .title{ background:url(workflow.axd?type=image&amp;src=tools.png) no-repeat; height:16px; border:solid 1px #9997b5;  }
        ///.Toolbar  [字符串的其余部分被截断]&quot;; 的本地化字符串。
        /// </summary>
        internal static string StyleSheet {
            get {
                return ResourceManager.GetString("StyleSheet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找 System.Drawing.Bitmap 类型的本地化资源。
        /// </summary>
        internal static System.Drawing.Bitmap tools {
            get {
                object obj = ResourceManager.GetObject("tools", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   查找类似 &lt;!DOCTYPE html&gt;
        ///&lt;html&gt;
        ///
        ///&lt;head&gt;
        ///    &lt;title&gt;工作流设计器&lt;/title&gt;
        ///    &lt;meta charset=&quot;utf-8&quot; /&gt;
        ///    &lt;link href=&quot;?type=style&quot; rel=&quot;stylesheet&quot; /&gt;
        ///    &lt;script language=&quot;javascript&quot; type=&quot;text/javascript&quot; src=&quot;?ac=js&amp;file=mootools.js&quot;&gt;&lt;/script&gt;
        ///    &lt;script language=&quot;javascript&quot; type=&quot;text/javascript&quot; src=&quot;?ac=js&amp;file=moostudio.js&quot;&gt;&lt;/script&gt;
        ///    &lt;script src=&quot;?type=script&quot; type=&quot;text/javascript&quot;&gt;&lt;/script&gt;
        ///    &lt;script language=&quot;javascript&quot; type=&quot;text/javascript&quot;&gt;
        ///        function loadLink(url) {
        ///            var [字符串的其余部分被截断]&quot;; 的本地化字符串。
        /// </summary>
        internal static string workflow {
            get {
                return ResourceManager.GetString("workflow", resourceCulture);
            }
        }
    }
}