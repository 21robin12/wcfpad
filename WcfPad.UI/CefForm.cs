using CefSharp;
using CefSharp.WinForms;
using Ninject;
using RazorEngine;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WcfPad.UI.Controllers;

namespace WcfPad.UI
{
    public partial class CefForm : Form
    {
        public static CefForm Instance;

        private readonly ChromiumWebBrowser _browser;

        public CefForm()
        {
            InitializeComponent();
            Text = "WcfPad";

            var settings = new CefSettings();
            Cef.Initialize(settings);

            _browser = new ChromiumWebBrowser(string.Empty) { Dock = DockStyle.Fill };
            browserPanel.Controls.Add(_browser);

            Icon = new Icon(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico"));

            var bounds = Screen.FromControl(this).Bounds; // screen res - need to be DPI-aware
            Width = (int)(bounds.Width * 0.6);
            Height = (int)(bounds.Height * 0.7);

            _browser.IsBrowserInitializedChanged += BrowserOnIsBrowserInitializedChanged;
            _browser.RegisterAsyncJsObject("csharpAjax", new AjaxObject());

            _browser.BrowserSettings.UniversalAccessFromFileUrls = CefState.Enabled;
            _browser.BrowserSettings.FileAccessFromFileUrls = CefState.Enabled;

            Instance = this;
        }

        public void Render<T>(T model, string view)
        {
            var service = Engine.Razor;

            var templates = new[]
            {
                "Views/Shared/_Layout.cshtml"
            };

            foreach (var t in templates)
            {
                var layout = File.ReadAllText(t);
                service.AddTemplate(t, layout);
            }

            // Note that "key" is the template key, which Razor uses to cache the compiled result.
            // We can re-run the cached template using this same key: var html = Engine.Razor.Run("key", typeof(MyViewModel), vm); 
            var key = view;
            var template = File.ReadAllText(view);
            var html = Engine.Razor.RunCompile(template, key, typeof(T), model);

            // "\\placeholder" tricks CefSharp into using the entire directory path; otherwise it strips off the last component
            // TODO fix this - maybe I'm just linking incorrectly
            var directory = Directory.GetCurrentDirectory() + "\\placeholder";
            _browser.LoadHtml(html, directory);
        }

        public void ExecuteJsFunction(string function, params object[] args)
        {
            _browser.ExecuteScriptAsync(function, args);
        }

        private void BrowserOnIsBrowserInitializedChanged(object sender, IsBrowserInitializedChangedEventArgs isBrowserInitializedChangedEventArgs)
        {
            if (_browser.IsBrowserInitialized)
            {
                if (CustomCefSettings.ShowDevTools)
                {
                    _browser.ShowDevTools();
                }

                var controller = DependencyInjection.Get<MainPageController>();
                controller.MainPage();
            }
        }
    }
}
