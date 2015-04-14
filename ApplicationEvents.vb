﻿'    WakeOnLAN - Wake On LAN
'    Copyright (C) 2004-2015 Aquila Technology, LLC. <webmaster@aquilatech.com>
'
'    This file is part of WakeOnLAN.
'
'    WakeOnLAN is free software: you can redistribute it and/or modify
'    it under the terms of the GNU General Public License as published by
'    the Free Software Foundation, either version 3 of the License, or
'    (at your option) any later version.
'
'    WakeOnLAN is distributed in the hope that it will be useful,
'    but WITHOUT ANY WARRANTY; without even the implied warranty of
'    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'    GNU General Public License for more details.
'
'    You should have received a copy of the GNU General Public License
'    along with WakeOnLAN.  If not, see <http://www.gnu.org/licenses/>.

Imports System.Globalization
Imports Localization
Imports AlphaWindow
Imports System.Runtime.InteropServices
Imports Microsoft.Win32

Namespace My

    ' The following events are available for MyApplication:
    ' 
    ' Startup: Raised when the application starts, before the startup form is created.
    ' Shutdown: Raised after all application forms are closed.  This event is not raised if the application terminates abnormally.
    ' UnhandledException: Raised if the application encounters an unhandled exception.
    ' StartupNextInstance: Raised when launching a single-instance application and the application is already active. 
    ' NetworkAvailabilityChanged: Raised when the network connection is connected or disconnected.

    Partial Friend Class MyApplication
        <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)> _
        Private Shared Function FindWindow(ByVal lpClassName As String, ByVal lpWindowName As String) As IntPtr
        End Function

        <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)> _
        Private Shared Function ShowWindow(ByVal hwnd As IntPtr, ByVal nCmdShow As Int32) As Boolean
        End Function

        ' Defines:
        ' DISPLAY  - used to zero out the last part of MAC addresses for screenshots

        Private Sub MyApplication_Startup(ByVal sender As Object, ByVal e As Microsoft.VisualBasic.ApplicationServices.StartupEventArgs) Handles Me.Startup
            If (Control.ModifierKeys = Keys.Control) Then
                SafeMode.ShowDialog()
            End If

            Debug.WriteLine("CommonApplicationData: " & Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData))

            singleInstance()
            upgradeSettings()
            ConfigureCulture()

            Dim version As String = System.String.Format(Resources.Strings.Version, Application.Info.Version.Major, Application.Info.Version.Minor, Application.Info.Version.Build, Application.Info.Version.Revision)

            If (Application.Info.Version.Revision > 0) Then
                version &= " BETA " & Application.Info.Version.Revision
            End If
            Splash.ShowSplash(Resources.Splash, Resources.Strings.Title, version, Application.Info.Copyright)

        End Sub

        Private Sub MyApplication_UnhandledException(ByVal sender As Object, ByVal e As ApplicationServices.UnhandledExceptionEventArgs) Handles Me.UnhandledException
            Dim crash As New Crash()

            crash.exception = e.Exception
            crash.ShowDialog()
            Application.Log.WriteException(e.Exception, TraceEventType.Critical, "Application shut down at " & Computer.Clock.GmtTime.ToString)
        End Sub

        ''' <summary>
        ''' Setup the culture and language configuration.
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub ConfigureCulture()
            Dim regKey As Microsoft.Win32.RegistryKey
            Dim language As String

            regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\Aquila Technology\WakeOnLAN", RegistryKeyPermissionCheck.ReadWriteSubTree)
            language = regKey.GetValue("Language", String.Empty, Microsoft.Win32.RegistryValueOptions.None)

            Select Case language
                Case "en"
                    language = "en-US"
                Case "de"
                    language = "de-DE"
                Case "es"
                    language = "es-ES"
                Case "fi"
                    language = "fi-FI"
                Case "fr"
                    language = "fr-FR"
                Case "hu"
                    language = "hu-HU"
                Case "nl"
                    language = "nl-NL"
                Case "ru"
                    language = "ru-RU"
                Case "pt_BR"
                    language = "pt-BR"
                Case "zh_TW"
                    language = "zh-TW"
            End Select

            If String.IsNullOrEmpty(language) Then
                language = "en-US"
            Else
                Settings.Language = language
                regKey.DeleteValue("Language")
            End If
            regKey.Close()

            If String.IsNullOrEmpty(Settings.Language) Then Settings.Language = language
            CultureManager.ApplicationUICulture = New CultureInfo(Settings.Language)
            Debug.WriteLine(Settings.Language)
        End Sub

        ''' <summary>
        ''' Search for another running instance of WOL.  If found, activate it and terminate this instance.
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub singleInstance()
            Const SW_RESTORE As Int32 = 9
            Const SW_SHOW As Int32 = 5
            Dim hwnd As IntPtr

            hwnd = FindWindow(Nothing, Resources.Strings.Title)

            If hwnd <> IntPtr.Zero Then
                ShowWindow(hwnd, SW_SHOW)
                ShowWindow(hwnd, SW_RESTORE)
                SetForegroundWindow(hwnd)
                End
            End If

        End Sub

        ''' <summary>
        ''' If this is an upgrade from a previous version, try to recover the previous user settings.
        ''' </summary>
        ''' <remarks></remarks>
        Private Sub upgradeSettings()
            Dim regKey As Microsoft.Win32.RegistryKey
            Dim database As String
            Dim newPath As String
            Dim filename As String

            If Settings.needUpgrade Then
                Settings.Upgrade()
                Settings.needUpgrade = False
                Settings.Save()
            End If

            Try
                regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\Aquila Technology\WakeOnLAN")
                database = regKey.GetValue("Database", IO.Directory.GetParent(Computer.FileSystem.SpecialDirectories.AllUsersApplicationData.ToString).ToString, Microsoft.Win32.RegistryValueOptions.None)
                regKey.Close()

                filename = IO.Path.GetFileName(Settings.dbPath)
                If (String.IsNullOrEmpty(filename)) Then
                    filename = "machines.xml"
                End If
                newPath = IO.Path.Combine(database, filename)

                If (Settings.dbPath <> newPath) Then
                    Settings.dbPath = newPath
                    Settings.Save()
                End If

            Catch ex As Exception

            End Try

        End Sub
    End Class

End Namespace

