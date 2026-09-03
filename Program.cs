using System;
using System.IO;
using System.Windows.Forms;

namespace WindowsToolkit;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        CreateFolders();

        Application.Run(
            new Form1()
        );
    }


    private static void CreateFolders()
    {
        string root =
            AppContext.BaseDirectory;


        string appTool =
            Path.Combine(
                root,
                "appTool"
            );


        string config =
            Path.Combine(
                root,
                "Config"
            );


        string icons =
            Path.Combine(
                root,
                "Icons"
            );


        string backup =
            Path.Combine(
                root,
                "Backup"
            );


        string updates =
            Path.Combine(
                root,
                "Updates"
            );


        Directory.CreateDirectory(
            appTool
        );

        Directory.CreateDirectory(
            config
        );

        Directory.CreateDirectory(
            icons
        );

        Directory.CreateDirectory(
            backup
        );

        Directory.CreateDirectory(
            updates
        );


        Directory.CreateDirectory(
            Path.Combine(
                backup,
                "BeforeUpdate"
            )
        );


        Directory.CreateDirectory(
            Path.Combine(
                backup,
                "UserBackup"
            )
        );


        for (
            int i = 1;
            i <= 8;
            i++
        )
        {
            Directory.CreateDirectory(
                Path.Combine(
                    appTool,
                    $"Muc{i}"
                )
            );
        }
    }
}