//using System;
using System.Diagnostics;
//using System.Reflection;
//using System.Windows;

namespace Kapok.View;

/// <summary>
/// Generates an resource path to an image.
/// </summary>
public static class ImageManager
{
    /*public enum Dpi
    {
        Dpi96,
        Dpi120,
        Dpi144,
        Dpi192
    }*/

    public enum ImageSize
    {
        Small,
        Large
    }

    /*public static Dpi ApplicationDpi
    {
        get
        {
            throw new NotImplementedException();
            
            // source: http://stackoverflow.com/questions/1918877/how-can-i-get-the-dpi-in-wpf
            var dpiYProperty = typeof(SystemParameters).GetProperty("Dpi", BindingFlags.NonPublic | BindingFlags.Static);
            Debug.Assert(dpiYProperty != null, nameof(dpiYProperty) + " != null");
            var dpiY = (int)dpiYProperty.GetValue(null, null);

            switch (dpiY)
            {
                case 96:
                    return Dpi.Dpi96;
                case 120:
                    return Dpi.Dpi120;
                case 144:
                    return Dpi.Dpi144;
                case 192:
                    return Dpi.Dpi192;
                default:
                    return Dpi.Dpi96;
            }
        }
    }*/

    public static string GetImageResource(string name, ImageSize size)
    {
        // see https://msdn.microsoft.com/en-us/library/windows/desktop/dd316921(v=vs.85).aspx
        /*int requiredBitSize;

        switch (size)
        {
            case ImageSize.Small:
                switch (ApplicationDpi)
                {
                    case Dpi.Dpi96:
                        requiredBitSize = 16;
                        break;
                    case Dpi.Dpi120:
                        requiredBitSize = 20;
                        break;
                    case Dpi.Dpi144:
                        requiredBitSize = 24;
                        break;
                    case Dpi.Dpi192:
                        requiredBitSize = 32;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                break;
            case ImageSize.Large:
                switch (ApplicationDpi)
                {
                    case Dpi.Dpi96:
                        requiredBitSize = 32;
                        break;
                    case Dpi.Dpi120:
                        requiredBitSize = 40;
                        break;
                    case Dpi.Dpi144:
                        requiredBitSize = 48;
                        break;
                    case Dpi.Dpi192:
                        requiredBitSize = 64;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(size), size, null);
        }*/


        // reduce the size to the available image sizes...
        /* TODO To be done when I bought the real icons...
        int imageSize;
        if (requiredBitSize <= 16)
            imageSize = 16;
        else if (requiredBitSize <= 24)
            imageSize = 24;
        else if (requiredBitSize <= 32)
            imageSize = 32;
        else
            imageSize = 48;
        */
        string imageSize = size.ToString().ToLower();

        switch (name)
        {
            // from https://icons8.com/
            case "courthouse":
            // other source
            case "account-book":
            case "account-statement":
            case "address-book-business":
            case "bank":
            case "bank-exchange":
            case "book":
            case "buildings":
            case "button-add":
            case "button-add-browse":
            case "button-cancel":
            case "button-error":
            case "button-help":
            case "button-info":
            case "button-ok":
            case "command-redo":
            case "command-redo-gray":
            case "command-undo":
            case "command-undo-gray":
            case "company-building":
            case "currency-exchange":
            case "datev":
            case "document-new":
            case "document-arrow-down":
            case "document-edit":
            case "execute":
            case "export-to-excel":
            case "filter":
            case "filter-cancel-2":
            case "filter-edit":
            case "note":
            case "note-text":
            case "report":
            case "report-book":
            case "report-print":
            case "review-track-changes":
            case "save":
            case "sort":
            case "sort-az":
            case "sort-down":
            case "sort-up":
            case "sort-za": 
            case "symbol-delete":
            case "symbol-question-mark":
            case "symbol-refresh":
            case "table-import":
            case "table-row-delete":
            case "table-row-down":
            case "table-row-edit":
            case "table-row-extract":
            case "table-row-insert":
            case "table-row-new":
            case "table-row-up":
            case "tag-blue":
            case "tag-gray":
            case "tag-green":
            case "tag-red":
            case "tool-pencil":
            case "trash":
            case "user-group":
            case "user":
            case "view-details":
            case "window":
                return BuildResourceString($"{name}_{imageSize}.png");
                
            case "TODO":
            case "@TODO":
                Debug.WriteLine($"!! TODO Image found");
                return BuildResourceString($"window_{imageSize}.png"); // TODO needs to be implemented
            default:
                Debug.WriteLine($"!! Image not found: {name}");
                return BuildResourceString($"symbol-question-mark_{imageSize}.png");
            //throw new ArgumentOutOfRangeException(nameof(name), name, null);
        }
    }

    private static string BuildResourceString(string fileName)
    {
        return $"pack://application:,,,/Kapok.View.Wpf;component/Resources/Icons/{fileName}";
    }
}