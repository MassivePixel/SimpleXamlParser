﻿// Copyright (c) Massive Pixel.  All Rights Reserved.  Licensed under the MIT License (MIT). See License.txt in the project root for license information.

using System;
using System.Linq;
using Sancho.DOM.XamarinForms;
using TabletDesigner.Helpers;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace TabletDesigner
{
    public enum EditorArea
    {
        XAML,
        JSON
    }

    public partial class TabletDesignerPage : ContentPage
    {
        ILogAccess logAccess;
        EditorArea currentArea;

        ContentViewInjector cvi;

        public TabletDesignerPage()
        {
            InitializeComponent();
            logAccess = DependencyService.Get<ILogAccess>();

            cvi = new ContentViewInjector(Root);

            var cp = new ContentPage();
            cp.LoadFromXaml(@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<ContentPage xmlns=""http://xamarin.com/schemas/2014/forms""
             xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
             x:Class=""XamlSamples.SliderBindingsPage""
             Title=""Slider Bindings Page"">

  <StackLayout>
    <Label Text=""ROTATION""
           BindingContext=""{x:Reference Name=slider}""
           Rotation=""{Binding Path=Value}""
           FontAttributes=""Bold""
           FontSize=""Large""
           HorizontalOptions=""Center""
           VerticalOptions=""CenterAndExpand"" />

    <Slider x:Name=""slider""
            Maximum=""360""
            VerticalOptions=""CenterAndExpand"" />

    <Label BindingContext=""{x:Reference slider}""
          Text=""{Binding Value,
                          StringFormat='The angle is {0:F0} degrees'}""
          FontAttributes=""Bold""
          FontSize=""Large""
          HorizontalOptions=""Center""
          VerticalOptions=""CenterAndExpand"" />
  </StackLayout>
</ContentPage>");
            var sl = cp.Content as StackLayout;
            var label = sl?.Children
                           ?.OfType<Label>()
                           ?.LastOrDefault();
            var props = label?.GetProperties();

            var slider = NameScopeExtensions.FindByName<Slider>(cp, "slider");
            var ns = NameScope.GetNameScope(cp);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            editor.Text = Settings.Xaml;
            HandleJsonChanges(Settings.Json);
        }

        void Editor_TextChanged(object sender, TextChangedEventArgs e)
        {
            switch (currentArea)
            {
                case EditorArea.XAML:
                    Settings.Xaml = editor.Text;
                    HandleXamlChanges(e.NewTextValue);
                    break;

                case EditorArea.JSON:
                    Settings.Json = editor.Text;
                    HandleJsonChanges(e.NewTextValue);
                    break;
            }
        }

        void HandleXamlChanges(string text)
        {
            try
            {
                logAccess.Clear();

                cvi.SetXaml(text);

                LoggerOutput.FormattedText = FormatLog(logAccess.Log);
                LoggerOutput.TextColor = Color.White;
            }
            catch (Exception ex)
            {
                LoggerOutput.FormattedText = FormatException(ex);
                LoggerOutput.TextColor = Color.FromHex("#FF3030");
            }
        }

        void HandleJsonChanges(string text)
        {
            try
            {
                logAccess.Clear();

                cvi.SetBindingContext(JsonModel.Parse(text));
            }
            catch (Exception ex)
            {
                LoggerOutput.FormattedText = FormatException(ex);
                LoggerOutput.TextColor = Color.FromHex("#FF3030");
            }
        }

        FormattedString FormatLog(string log)
        {
            var fs = new FormattedString();
            var lines = log.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                fs.Spans.Add(new Span
                {
                    Text = line + Environment.NewLine,
                    FontFamily = LoggerOutput.FontFamily,
                    FontSize = LoggerOutput.FontSize,
                    ForegroundColor = GetOutputColor(line)
                });
            }
            return fs;
        }

        Color GetOutputColor(string line)
        {
            if (line.Contains("[Warning]"))
                return Color.FromHex("#FFA500");
            if (line.Contains("[Error]"))
                return Color.Red;

            return Color.White;
        }

        FormattedString FormatException(Exception ex)
        {
            var fs = new FormattedString();
            while (ex != null)
            {
                fs.Spans.Add(new Span
                {
                    Text = ex.Message + Environment.NewLine,
                    ForegroundColor = Color.Red,
                    FontFamily = LoggerOutput.FontFamily,
                    FontSize = LoggerOutput.FontSize,
                });
                fs.Spans.Add(new Span
                {
                    Text = ex.StackTrace + Environment.NewLine,
                    ForegroundColor = Color.Red,
                    FontFamily = LoggerOutput.FontFamily,
                    FontSize = LoggerOutput.FontSize,
                });

                if (ex.InnerException != null)
                {
                    fs.Spans.Add(new Span
                    {
                        Text = "--" + Environment.NewLine,
                        ForegroundColor = Color.Red,
                        FontFamily = LoggerOutput.FontFamily,
                        FontSize = LoggerOutput.FontSize,
                    });
                }

                ex = ex.InnerException;
            }
            return fs;
        }

        void BtnXaml_Clicked(object sender, EventArgs args)
        {
            editor.TextChanged -= Editor_TextChanged;
            editor.Text = Settings.Xaml;
            currentArea = EditorArea.XAML;
            editor.TextChanged += Editor_TextChanged;
        }

        void BtnJson_Clicked(object sender, EventArgs args)
        {
            editor.TextChanged -= Editor_TextChanged;
            editor.Text = Settings.Json;
            currentArea = EditorArea.JSON;
            editor.TextChanged += Editor_TextChanged;
        }
    }
}
