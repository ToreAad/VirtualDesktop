// Author: Markus Scholtes, 2021
// Version 1.9, 2021-10-08
// Version for Windows 10 1809 to 21H1
// Compile with:
// C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe VirtualDesktop.cs

using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Text;

// set attributes
using System.Reflection;
[assembly:AssemblyTitle("Command line tool to manage virtual desktops")]
[assembly:AssemblyDescription("Command line tool to manage virtual desktops")]
[assembly:AssemblyConfiguration("")]
[assembly:AssemblyCompany("MS")]
[assembly:AssemblyProduct("VirtualDesktop")]
[assembly:AssemblyCopyright("© Markus Scholtes 2021")]
[assembly:AssemblyTrademark("")]
[assembly:AssemblyCulture("")]
[assembly:AssemblyVersion("1.9.0.0")]
[assembly:AssemblyFileVersion("1.9.0.0")]

// Based on http://stackoverflow.com/a/32417530, Windows 10 SDK, github project Grabacr07/VirtualDesktop and own research

namespace VDeskTool
{
	static class Program
	{
		static bool verbose = true;
		static bool breakonerror = true;
		static bool wrapdesktops = false;
		static int rc = 0;

		static int Main(string[] args)
		{
			if (args.Length == 0)
			{ // no arguments, show help screen
				HelpScreen();
				return -2;
			}

			foreach (string arg in args)
			{
				System.Text.RegularExpressions.GroupCollection groups = System.Text.RegularExpressions.Regex.Match(arg, @"^[-\/]?([^:=]+)[:=]?(.*)$").Groups;

				if (groups.Count != 3)
				{ // parameter error
					rc = -2;
				}
				else
				{ // reset return code if on error
					if (rc < 0) rc = 0;

					if (groups[2].Value == "")
					{ // parameter without value
						switch(groups[1].Value.ToUpper())
						{
							case "HELP": // help screen
							case "H":
							case "?":
								HelpScreen();
								return 0;

							case "QUIET": // don't display messages
							case "Q":
								verbose = false;
								break;

							case "VERBOSE": // display messages
							case "V":
								Console.WriteLine("Verbose mode enabled");
								verbose = true;
								break;

							case "BREAK": // break on error
							case "B":
								if (verbose) Console.WriteLine("Break on error enabled");
								breakonerror = true;
								break;

							case "CONTINUE": // continue on error
							case "CO":
								if (verbose) Console.WriteLine("Break on error disabled");
								breakonerror = false;
								break;

							case "WRAP": // wrap desktops using "LEFT" or "RIGHT"
							case "W":
								if (verbose) Console.WriteLine("Wrapping desktops enabled");
								wrapdesktops = true;
								break;

							case "NOWRAP": // do not wrap desktops
							case "NW":
								if (verbose) Console.WriteLine("Wrapping desktop disabled");
								wrapdesktops = false;
								break;

							case "COUNT": // get count of desktops
							case "C":
								rc = VirtualDesktop.Desktop.Count;
								if (verbose) Console.WriteLine("Count of desktops: " + rc);
								break;

							case "LIST": // show list of desktops
							case "LI":
								int desktopCount = VirtualDesktop.Desktop.Count;
								int visibleDesktop = VirtualDesktop.Desktop.FromDesktop(VirtualDesktop.Desktop.Current);
								if (verbose)
								{
									Console.WriteLine("Virtual desktops:");
									Console.WriteLine("-----------------");
								}
								for (int i = 0; i < desktopCount; i++)
								{
									if (i != visibleDesktop)
										Console.WriteLine(VirtualDesktop.Desktop.DesktopNameFromIndex(i));
									else
										Console.WriteLine(VirtualDesktop.Desktop.DesktopNameFromIndex(i) + " (visible)");
								}
								if (verbose) Console.WriteLine("\nCount of desktops: " + desktopCount);
								break;

							case "GETCURRENTDESKTOP": // get number of current desktop and display desktop name
							case "GCD":
								rc = VirtualDesktop.Desktop.FromDesktop(VirtualDesktop.Desktop.Current);
								if (verbose) Console.WriteLine("Current desktop: '" + VirtualDesktop.Desktop.DesktopNameFromDesktop(VirtualDesktop.Desktop.Current) + "' (desktop number " + rc + ")");
								break;

							case "ISVISIBLE": // is desktop in rc visible?
							case "IV":
								if ((rc >= 0) && (rc < VirtualDesktop.Desktop.Count))
								{ // check if parameter is in range of active desktops
									if (VirtualDesktop.Desktop.FromIndex(rc).IsVisible)
									{
										if (verbose) Console.WriteLine("Virtual desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(rc) + "' (desktop number " + rc.ToString() + ") is visible");
										rc = 0;
									}
									else
									{
										if (verbose) Console.WriteLine("Virtual desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(rc) + "' (desktop number " + rc.ToString() + ") is not visible");
										rc = 1;
									}
								}
								else
									rc = -1;
								break;

							case "SWITCH": // switch to desktop in rc
							case "S":
								if (verbose) Console.Write("Switching to virtual desktop number " + rc.ToString());
								try
								{ // activate virtual desktop rc
									VirtualDesktop.Desktop.FromIndex(rc).MakeVisible();
									if (verbose) Console.WriteLine(", desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(rc) + "' is active now");
								}
								catch
								{ // error while activating
									if (verbose) Console.WriteLine();
									rc = -1;
								}
								break;

							case "LEFT": // switch to desktop to the left
							case "L":
								if (verbose) Console.Write("Switching to left virtual desktop");
								try
								{ // activate virtual desktop to the left
									if (wrapdesktops && (VirtualDesktop.Desktop.FromDesktop(VirtualDesktop.Desktop.Current) == 0))
										VirtualDesktop.Desktop.FromIndex(VirtualDesktop.Desktop.Count - 1).MakeVisible();
									else
										VirtualDesktop.Desktop.Current.Left.MakeVisible();
									rc = VirtualDesktop.Desktop.FromDesktop(VirtualDesktop.Desktop.Current);
									if (verbose) Console.WriteLine(", desktop number " + rc.ToString() + " ('" + VirtualDesktop.Desktop.DesktopNameFromIndex(rc) + "') is active now");
								}
								catch
								{ // error while activating
									if (verbose) Console.WriteLine();
									rc = -1;
								}
								break;

							case "RIGHT": // switch to desktop to the right
							case "RI":
								if (verbose) Console.Write("Switching to right virtual desktop");
								try
								{ // activate virtual desktop to the right
									if (wrapdesktops && (VirtualDesktop.Desktop.FromDesktop(VirtualDesktop.Desktop.Current) == VirtualDesktop.Desktop.Count - 1))
										VirtualDesktop.Desktop.FromIndex(0).MakeVisible();
									else
										VirtualDesktop.Desktop.Current.Right.MakeVisible();
									rc = VirtualDesktop.Desktop.FromDesktop(VirtualDesktop.Desktop.Current);
									if (verbose) Console.WriteLine(", desktop number " + rc.ToString() + " ('" + VirtualDesktop.Desktop.DesktopNameFromIndex(rc) + "') is active now");
								}
								catch
								{ // error while activating
									if (verbose) Console.WriteLine();
									rc = -1;
								}
								break;

							case "NEW": // create new desktop
							case "N":
								if (verbose) Console.Write("Creating virtual desktop");
								try
								{ // create virtual desktop, number is stored in rc
									rc = VirtualDesktop.Desktop.FromDesktop(VirtualDesktop.Desktop.Create());
									if (verbose) Console.WriteLine(" number " + rc.ToString());
								}
								catch
								{ // error while creating
									Console.WriteLine();
									rc = -1;
								}
								break;

							case "REMOVE": // remove desktop in rc
							case "R":
								if (verbose)
								{
									Console.Write("Removing virtual desktop number " + rc.ToString());
									if ((rc >= 0) && (rc < VirtualDesktop.Desktop.Count)) Console.WriteLine(" (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(rc) + "')");
								}
								try
								{ // remove virtual desktop rc
									VirtualDesktop.Desktop.FromIndex(rc).Remove();
								}
								catch
								{ // error while removing
									Console.WriteLine();
									rc = -1;
								}
								break;

							case "MOVEACTIVEWINDOW": // move active window to desktop in rc
							case "MAW":
								if (verbose) Console.WriteLine("Moving active window to virtual desktop number " + rc.ToString());
								try
								{ // move active window
									VirtualDesktop.Desktop.FromIndex(rc).MoveActiveWindow();
									if (verbose) Console.WriteLine("Active window moved to desktop number " + rc.ToString() + " (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(rc) + "')");
								}
								catch
								{ // error while moving
									if (verbose) Console.WriteLine("No active window or move failed");
									rc = -1;
								}
								break;

							case "WAITKEY": // wait for keypress
							case "WK":
								if (verbose) Console.WriteLine("Press a key");
								Console.ReadKey(true);
								break;

							default:
								rc = -2;
								break;
						}
					}
					else
					{	// parameter with value
						int iParam;

						switch(groups[1].Value.ToUpper())
						{
							case "GETDESKTOP": // get desktop number
							case "GD":
								if (int.TryParse(groups[2].Value, out iParam))
								{ // parameter is an integer, use as desktop number
									if ((iParam >= 0) && (iParam < VirtualDesktop.Desktop.Count))
									{ // check if parameter is in range of active desktops
										if (verbose) Console.WriteLine("Virtual desktop number " + iParam.ToString() + " (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(iParam) + "') selected");
										rc = iParam;
									}
									else
										rc = -1;
								}
								else
								{ // parameter is a string, search as part of desktop name
									iParam = VirtualDesktop.Desktop.SearchDesktop(groups[2].Value);
									if (iParam >= 0)
									{ // desktop found
										if (verbose) Console.WriteLine("Virtual desktop number " + iParam.ToString() + " (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(iParam) + "') selected");
										rc = iParam;
									}
									else
									{ // no desktop found
										if (verbose) Console.WriteLine("Could not find virtual desktop with name containing '" + groups[2].Value + "'");
										rc = -2;
									}
								}
								break;

							case "ISVISIBLE": // is desktop visible?
							case "IV":
								if (int.TryParse(groups[2].Value, out iParam))
								{ // parameter is an integer, use as desktop number
									if ((iParam >= 0) && (iParam < VirtualDesktop.Desktop.Count))
									{ // check if parameter is in range of active desktops
										if (VirtualDesktop.Desktop.FromIndex(iParam).IsVisible)
										{
											if (verbose) Console.WriteLine("Virtual desktop number " + iParam.ToString() + " (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(iParam) + "') is visible");
											rc = 0;
										}
										else
										{
											if (verbose) Console.WriteLine("Virtual desktop number " + iParam.ToString() + " (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(iParam) + "') is not visible");
											rc = 1;
										}
									}
									else
										rc = -1;
								}
								else
								{ // parameter is a string, search as part of desktop name
									iParam = VirtualDesktop.Desktop.SearchDesktop(groups[2].Value);
									if (iParam >= 0)
									{ // desktop found
										if (VirtualDesktop.Desktop.FromIndex(iParam).IsVisible)
										{
											if (verbose) Console.WriteLine("Virtual desktop number " + iParam.ToString() + " (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(iParam) + "') is visible");
											rc = 0;
										}
										else
										{
											if (verbose) Console.WriteLine("Virtual desktop number " + iParam.ToString() + " (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(iParam) + "') is not visible");
											rc = 1;
										}
									}
									else
									{ // no desktop found
										if (verbose) Console.WriteLine("Could not find virtual desktop with name containing '" + groups[2].Value + "'");
										rc = -2;
									}
								}
								break;

							case "SWITCH": // switch to desktop
							case "S":
								if (int.TryParse(groups[2].Value, out iParam))
								{ // parameter is an integer, use as desktop number
									if ((iParam >= 0) && (iParam < VirtualDesktop.Desktop.Count))
									{ // check if parameter is in range of active desktops
										if (verbose) Console.WriteLine("Switching to virtual desktop number " + iParam.ToString() + " (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(iParam) + "')");
										rc = iParam;
										try
										{ // activate virtual desktop iParam
											VirtualDesktop.Desktop.FromIndex(iParam).MakeVisible();
										}
										catch
										{ // error while activating
											rc = -1;
										}
									}
									else
										rc = -1;
								}
								else
								{ // parameter is a string, search as part of desktop name
									iParam = VirtualDesktop.Desktop.SearchDesktop(groups[2].Value);
									if (iParam >= 0)
									{ // desktop found
										if (verbose) Console.WriteLine("Switching to virtual desktop number " + iParam.ToString() + " (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(iParam) + "')");
										rc = iParam;
										try
										{ // activate virtual desktop iParam
											VirtualDesktop.Desktop.FromIndex(iParam).MakeVisible();
										}
										catch
										{ // error while activating
											rc = -1;
										}
									}
									else
									{ // no desktop found
										if (verbose) Console.WriteLine("Could not find virtual desktop with name containing '" + groups[2].Value + "'");
										rc = -2;
									}
								}
								break;

							case "REMOVE": // remove desktop
							case "R":
								if (int.TryParse(groups[2].Value, out iParam))
								{ // parameter is an integer, use as desktop number
									if ((iParam >= 0) && (iParam < VirtualDesktop.Desktop.Count))
									{ // check if parameter is in range of active desktops
										if (verbose) Console.WriteLine("Removing virtual desktop number " + iParam.ToString() + " (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(iParam) + "')");
										rc = iParam;
										try
										{ // remove virtual desktop iParam
											VirtualDesktop.Desktop.FromIndex(iParam).Remove();
										}
										catch
										{ // error while removing
											rc = -1;
										}
									}
									else
										rc = -1;
								}
								else
								{ // parameter is a string, search as part of desktop name
									iParam = VirtualDesktop.Desktop.SearchDesktop(groups[2].Value);
									if (iParam >= 0)
									{ // desktop found
										if (verbose) Console.WriteLine("Removing virtual desktop number " + iParam.ToString() + " (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(iParam) + "')");
										rc = iParam;
										try
										{ // remove virtual desktop iParam
											VirtualDesktop.Desktop.FromIndex(iParam).Remove();
										}
										catch
										{ // error while removing
											rc = -1;
										}
									}
									else
									{ // no desktop found
										if (verbose) Console.WriteLine("Could not find virtual desktop with name containing '" + groups[2].Value + "'");
										rc = -2;
									}
								}
								break;

							case "SWAPDESKTOP": // swap desktops
							case "SD":
								if (int.TryParse(groups[2].Value, out iParam))
								{ // parameter is an integer, use as desktop number
									if ((iParam >= 0) && (iParam < VirtualDesktop.Desktop.Count) && (rc != iParam))
									{ // check if parameter is in range of active desktops
										if (verbose) Console.WriteLine("Swapping virtual desktops number " + rc.ToString() + " (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(rc) + "') and number " + iParam.ToString() + " (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(iParam) + "')");
										try
										{ // swap virtual desktops rc and iParam
											SwapDesktops(rc, iParam);
											rc = iParam;
										}
										catch
										{ // error while swapping
											rc = -1;
										}
									}
									else
										rc = -1;
								}
								else
								{ // parameter is a string, search as part of desktop name
									iParam = VirtualDesktop.Desktop.SearchDesktop(groups[2].Value);
									if ((iParam >= 0) && (rc != iParam))
									{ // desktop found
										if (verbose) Console.WriteLine("Swapping virtual desktops number " + rc.ToString() + " (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(rc) + "') and number " + iParam.ToString() + " (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(iParam) + "')");
										try
										{ // swap virtual desktops rc and iParam
											SwapDesktops(rc, iParam);
											rc = iParam;
										}
										catch
										{ // error while swapping
											rc = -1;
										}
									}
									else
									{ // no desktop found or source and target the same
										if (rc == iParam)
										{
											if (verbose) Console.WriteLine("Cannot swap virtual desktop with itself");
										}
										else
										{
											if (verbose) Console.WriteLine("Could not find virtual desktop with name containing '" + groups[2].Value + "'");
										}
										rc = -2;
									}
								}
								break;

							case "INSERTDESKTOP": // insert desktop
							case "ID":
								if (int.TryParse(groups[2].Value, out iParam))
								{ // parameter is an integer, use as desktop number
									if ((iParam >= 0) && (iParam < VirtualDesktop.Desktop.Count) && (rc != iParam))
									{ // check if parameter is in range of active desktops
										if (verbose) Console.WriteLine("Inserting virtual desktop number " + iParam.ToString() + " (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(iParam) + "') before desktop number " + rc.ToString() + " (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(rc) + "') or vice versa");
										try
										{ // insert virtual desktop iParam before rc
											InsertDesktop(rc, iParam);
											rc = iParam;
										}
										catch
										{ // error while inserting
											rc = -1;
										}
									}
									else
										rc = -1;
								}
								else
								{ // parameter is a string, search as part of desktop name
									iParam = VirtualDesktop.Desktop.SearchDesktop(groups[2].Value);
									if ((iParam >= 0) && (rc != iParam))
									{ // desktop found
										if (verbose) Console.WriteLine("Inserting virtual desktop number " + iParam.ToString() + " (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(iParam) + "') before desktop number " + rc.ToString() + " (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(rc) + "') or vice versa");
										try
										{ // insert virtual desktop iParam before rc
											InsertDesktop(rc, iParam);
											rc = iParam;
										}
										catch
										{ // error while inserting
											rc = -1;
										}
									}
									else
									{ // no desktop found or source and target the same
										if (rc == iParam)
										{
											if (verbose) Console.WriteLine("Cannot insert virtual desktop before itself");
										}
										else
										{
											if (verbose) Console.WriteLine("Could not find virtual desktop with name containing '" + groups[2].Value + "'");
										}
										rc = -2;
									}
								}
								break;

							case "GETDESKTOPFROMWINDOW": // get desktop from window
							case "GDFW":
								if (int.TryParse(groups[2].Value, out iParam))
								{ // check if parameter is an integer
									if (iParam > 0)
									{ // check if parameter is greater than 0
										try
										{ // seeking desktop for process handle
											iParam = (int)System.Diagnostics.Process.GetProcessById(iParam).MainWindowHandle;
											// process handle converted to window handle
											rc = VirtualDesktop.Desktop.FromDesktop(VirtualDesktop.Desktop.FromWindow((IntPtr)iParam));
											if (verbose) Console.WriteLine("Window is on desktop number " + rc.ToString() + " (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(rc) + "')");
										}
										catch
										{ // error while seeking
											if (verbose) Console.WriteLine("Window to process id " + groups[2].Value + "' not found");
											rc = -1;
										}
									}
									else
										rc = -1;
								}
								else
								{
									try
									{ // seeking desktop for process name
										iParam = GetMainWindowHandle(groups[2].Value.Trim());
										rc = VirtualDesktop.Desktop.FromDesktop(VirtualDesktop.Desktop.FromWindow((IntPtr)iParam));
										if (verbose) Console.WriteLine("Window of process '" + groups[2].Value + "' is on desktop number " + rc.ToString() + " (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(rc) + "')");
									}
									catch
									{ // error while seeking
										if (verbose) Console.WriteLine("Process '" + groups[2].Value + "' not found");
										rc = -1;
									}
								}
								break;

							case "GETDESKTOPFROMWINDOWHANDLE": // get desktop from window handle
							case "GDFWH":
								if (int.TryParse(groups[2].Value, out iParam))
								{ // check if parameter is an integer
									if (iParam > 0)
									{ // check if parameter is greater than 0
										try
										{ // seeking desktop for window handle
											rc = VirtualDesktop.Desktop.FromDesktop(VirtualDesktop.Desktop.FromWindow((IntPtr)iParam));
											if (verbose) Console.WriteLine("Window is on desktop number " + rc.ToString() + " (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(rc) + "')");
										}
										catch
										{ // error while seeking
											if (verbose) Console.WriteLine("Window to handle " + groups[2].Value + "' not found");
											rc = -1;
										}
									}
									else
										rc = -1;
								}
								else
								{
									try
									{ // seeking window with window title
										iParam = (Int32)GetWindowFromTitle(groups[2].Value.Trim().Replace("^", ""));
										// seeking desktop for window handle
										rc = VirtualDesktop.Desktop.FromDesktop(VirtualDesktop.Desktop.FromWindow((IntPtr)iParam));
										if (verbose) Console.WriteLine("Window '" + foundTitle + "' is on desktop number " + rc.ToString() + " (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(rc) + "')");
									}
									catch
									{ // error while seeking
										if (verbose) Console.WriteLine("Window with text '" + groups[2].Value + "' in title not found");
										rc = -1;
									}
								}
								break;

							case "ISWINDOWONDESKTOP": // is window on desktop in rc
							case "IWOD":
								if (int.TryParse(groups[2].Value, out iParam))
								{ // check if parameter is an integer
									if (iParam > 0)
									{ // check if parameter is greater than 0
										try
										{ // checking desktop for process handle
											iParam = (int)System.Diagnostics.Process.GetProcessById(iParam).MainWindowHandle;
											// process handle converted to window handle
											if (VirtualDesktop.Desktop.FromIndex(rc).HasWindow((IntPtr)iParam))
											{
												if (verbose) Console.WriteLine("Window to process id " + groups[2].Value + " is on desktop number " + rc.ToString() + " (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(rc) + "')");
												rc = 0;
											}
											else
											{
												if (verbose) Console.WriteLine("Window to process id " + groups[2].Value + " is not on desktop number " + rc.ToString() + " (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(rc) + "')");
												rc = 1;
											}
										}
										catch
										{ // error while seeking
											if (verbose) Console.WriteLine("Window to process id " + groups[2].Value + " not found");
											rc = -1;
										}
									}
									else
										rc = -1;
								}
								else
								{
									try
									{ // seeking desktop for process name
										iParam = GetMainWindowHandle(groups[2].Value.Trim());
										if (VirtualDesktop.Desktop.FromIndex(rc).HasWindow((IntPtr)iParam))
										{
											if (verbose) Console.WriteLine("Window of process '" + groups[2].Value + "' is on desktop number " + rc.ToString() + " (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(rc) + "')");
											rc = 0;
										}
										else
										{
											if (verbose) Console.WriteLine("Window of process '" + groups[2].Value + "' is not on desktop number " + rc.ToString() + " (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(rc) + "')");
											rc = 1;
										}
									}
									catch
									{ // error while seeking
										if (verbose) Console.WriteLine("Process '" + groups[2].Value + "' not found");
										rc = -1;
									}
								}
								break;

							case "ISWINDOWHANDLEONDESKTOP": // is window with handle on desktop in rc
							case "IWHOD":
								if (int.TryParse(groups[2].Value, out iParam))
								{ // check if parameter is an integer
									if (iParam > 0)
									{ // check if parameter is greater than 0
										try
										{ // checking desktop for window handle
											if (VirtualDesktop.Desktop.FromIndex(rc).HasWindow((IntPtr)iParam))
											{
												if (verbose) Console.WriteLine("Window to handle " + groups[2].Value + " is on desktop number " + rc.ToString() + " (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(rc) + "')");
												rc = 0;
											}
											else
											{
												if (verbose) Console.WriteLine("Window to handle " + groups[2].Value + " is not on desktop number " + rc.ToString() + " (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(rc) + "')");
												rc = 1;
											}
										}
										catch
										{ // error while seeking
											if (verbose) Console.WriteLine("Window to handle " + groups[2].Value + " not found");
											rc = -1;
										}
									}
									else
										rc = -1;
								}
								else
								{
									try
									{ // seeking window with window title
										iParam = (Int32)GetWindowFromTitle(groups[2].Value.Trim().Replace("^", ""));
										// checking desktop for window handle
										if (VirtualDesktop.Desktop.FromIndex(rc).HasWindow((IntPtr)iParam))
										{
											if (verbose) Console.WriteLine("Window '" + foundTitle + "' is on desktop number " + rc.ToString() + " (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(rc) + "')");
											rc = 0;
										}
										else
										{
											if (verbose) Console.WriteLine("Window '" + foundTitle + "' is not on desktop number " + rc.ToString() + " (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(rc) + "')");
											rc = 1;
										}
									}
									catch
									{ // error while seeking
										if (verbose) Console.WriteLine("Window with text '" + groups[2].Value + "' in title not found");
										rc = -1;
									}
								}
								break;

							case "MOVEWINDOW": // move window to desktop in rc
							case "MW":
								if (int.TryParse(groups[2].Value, out iParam))
								{ // check if parameter is an integer
									if (iParam > 0)
									{ // check if parameter is greater than 0
										try
										{ // seeking window for process handle
											iParam = (int)System.Diagnostics.Process.GetProcessById(iParam).MainWindowHandle;
											// process handle converted to window handle and move window
											VirtualDesktop.Desktop.FromIndex(rc).MoveWindow((IntPtr)iParam);
											if (verbose) Console.WriteLine("Window to process id " + groups[2].Value + " moved to desktop number " + rc.ToString() + " (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(rc) + "')");
										}
										catch
										{ // error while seeking
											if (verbose) Console.WriteLine("Window to process id " + groups[2].Value + " not found or move failed");
											rc = -1;
										}
									}
									else
										rc = -1;
								}
								else
								{
									try
									{ // seeking window for process name
										iParam = GetMainWindowHandle(groups[2].Value.Trim());
										// move window
										VirtualDesktop.Desktop.FromIndex(rc).MoveWindow((IntPtr)iParam);
										if (verbose) Console.WriteLine("Window of process '" + groups[2].Value + "' moved to desktop number " + rc.ToString() + " (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(rc) + "')");
									}
									catch
									{ // error while seeking
										if (verbose) Console.WriteLine("Process '" + groups[2].Value + "' not found or move failed");
										rc = -1;
									}
								}
								break;

							case "MOVEWINDOWHANDLE": // move window with handle to desktop in rc
							case "MWH":
								if (int.TryParse(groups[2].Value, out iParam))
								{ // check if parameter is an integer
									if (iParam > 0)
									{ // check if parameter is greater than 0
										try
										{
											// use window handle and move window
											VirtualDesktop.Desktop.FromIndex(rc).MoveWindow((IntPtr)iParam);
											if (verbose) Console.WriteLine("Window to handle " + groups[2].Value + " moved to desktop number " + rc.ToString() + " (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(rc) + "')");
										}
										catch
										{ // error while seeking
											if (verbose) Console.WriteLine("Window to handle " + groups[2].Value + " not found or move failed");
											rc = -1;
										}
									}
									else
										rc = -1;
								}
								else
								{
									try
									{ // seeking window with window title
										iParam = (Int32)GetWindowFromTitle(groups[2].Value.Trim().Replace("^", ""));
										// move window
										VirtualDesktop.Desktop.FromIndex(rc).MoveWindow((IntPtr)iParam);
										if (verbose) Console.WriteLine("Window '" + foundTitle + "' moved to desktop number " + rc.ToString() + " (desktop '" + VirtualDesktop.Desktop.DesktopNameFromIndex(rc) + "')");
									}
									catch
									{ // error while seeking
										if (verbose) Console.WriteLine("Window with text '" + groups[2].Value + "' in title not found or move failed");
										rc = -1;
									}
								}
								break;

							case "ISWINDOWPINNED": // is window pinned to all desktops
							case "IWP":
								if (int.TryParse(groups[2].Value, out iParam))
								{ // check if parameter is an integer
									if (iParam > 0)
									{ // check if parameter is greater than 0
										try
										{ // checking desktop for process handle
											iParam = (int)System.Diagnostics.Process.GetProcessById(iParam).MainWindowHandle;
											// process handle converted to window handle
											if (VirtualDesktop.Desktop.IsWindowPinned((IntPtr)iParam))
											{
												if (verbose) Console.WriteLine("Window to process id " + groups[2].Value + " is pinned to all desktops");
												rc = 0;
											}
											else
											{
												if (verbose) Console.WriteLine("Window to process id " + groups[2].Value + " is not pinned to all desktops");
												rc = 1;
											}
										}
										catch
										{ // error while seeking
											if (verbose) Console.WriteLine("Window to process id " + groups[2].Value + " not found");
											rc = -1;
										}
									}
									else
										rc = -1;
								}
								else
								{
									try
									{ // seeking desktop for process name
										iParam = GetMainWindowHandle(groups[2].Value.Trim());
										if (VirtualDesktop.Desktop.IsWindowPinned((IntPtr)iParam))
										{
											if (verbose) Console.WriteLine("Window of process '" + groups[2].Value + "' is pinned to all desktops");
											rc = 0;
										}
										else
										{
											if (verbose) Console.WriteLine("Window of process '" + groups[2].Value + "' is not pinned to all desktops");
											rc = 1;
										}
									}
									catch
									{ // error while seeking
										if (verbose) Console.WriteLine("Process '" + groups[2].Value + "' not found");
										rc = -1;
									}
								}
								break;

							case "ISWINDOWHANDLEPINNED": // is window with handle pinned to all desktops
							case "IWHP":
								if (int.TryParse(groups[2].Value, out iParam))
								{ // check if parameter is an integer
									if (iParam > 0)
									{ // check if parameter is greater than 0
										try
										{ // checking desktop for window handle
											if (VirtualDesktop.Desktop.IsWindowPinned((IntPtr)iParam))
											{
												if (verbose) Console.WriteLine("Window to handle " + groups[2].Value + " is pinned to all desktops");
												rc = 0;
											}
											else
											{
												if (verbose) Console.WriteLine("Window to handle " + groups[2].Value + " is not pinned to all desktops");
												rc = 1;
											}
										}
										catch
										{ // error while seeking
											if (verbose) Console.WriteLine("Window to handle " + groups[2].Value + " not found");
											rc = -1;
										}
									}
									else
										rc = -1;
								}
								else
								{
									try
									{ // seeking window with window title
										iParam = (Int32)GetWindowFromTitle(groups[2].Value.Trim().Replace("^", ""));
										if (VirtualDesktop.Desktop.IsWindowPinned((IntPtr)iParam))
										{
											if (verbose) Console.WriteLine("Window '" + foundTitle + "' is pinned to all desktops");
											rc = 0;
										}
										else
										{
											if (verbose) Console.WriteLine("Window '" + foundTitle + "' is not pinned to all desktops");
											rc = 1;
										}
									}
									catch
									{ // error while seeking
										if (verbose) Console.WriteLine("Window with text '" + groups[2].Value + "' in title not found");
										rc = -1;
									}
								}
								break;

							case "PINWINDOW": // pin window to all desktops
							case "PW":
								if (int.TryParse(groups[2].Value, out iParam))
								{ // check if parameter is an integer
									if (iParam > 0)
									{ // check if parameter is greater than 0
										try
										{ // seeking window for process handle
											iParam = (int)System.Diagnostics.Process.GetProcessById(iParam).MainWindowHandle;
											// process handle converted to window handle and pin window
											VirtualDesktop.Desktop.PinWindow((IntPtr)iParam);
											if (verbose) Console.WriteLine("Window to process id " + groups[2].Value + " pinned to all desktops");
										}
										catch
										{ // error while seeking
											if (verbose) Console.WriteLine("Window to process id " + groups[2].Value + " not found or pin failed");
											rc = -1;
										}
									}
									else
										rc = -1;
								}
								else
								{
									try
									{ // seeking window for process name
										iParam = GetMainWindowHandle(groups[2].Value.Trim());
										// pin window
										VirtualDesktop.Desktop.PinWindow((IntPtr)iParam);
										if (verbose) Console.WriteLine("Window of process '" + groups[2].Value + "' pinned to all desktops");
									}
									catch
									{ // error while seeking
										if (verbose) Console.WriteLine("Process '" + groups[2].Value + "' not found or pin failed");
										rc = -1;
									}
								}
								break;

							case "PINWINDOWHANDLE": // pin window with handle to all desktops
							case "PWH":
								if (int.TryParse(groups[2].Value, out iParam))
								{ // check if parameter is an integer
									if (iParam > 0)
									{ // check if parameter is greater than 0
										try
										{ // process window handle and pin window
											VirtualDesktop.Desktop.PinWindow((IntPtr)iParam);
											if (verbose) Console.WriteLine("Window to handle " + groups[2].Value + " pinned to all desktops");
										}
										catch
										{ // error while seeking
											if (verbose) Console.WriteLine("Window to handle " + groups[2].Value + " not found or pin failed");
											rc = -1;
										}
									}
									else
										rc = -1;
								}
								else
								{
									try
									{ // seeking window with window title
										iParam = (Int32)GetWindowFromTitle(groups[2].Value.Trim().Replace("^", ""));
										// pin window
										VirtualDesktop.Desktop.PinWindow((IntPtr)iParam);
										if (verbose) Console.WriteLine("Window '" + foundTitle + "' pinned to all desktops");
									}
									catch
									{ // error while seeking
										if (verbose) Console.WriteLine("Window with text '" + groups[2].Value + "' in title not found or pin failed");
										rc = -1;
									}
								}
								break;

							case "UNPINWINDOW": // unpin window from all desktops
							case "UPW":
								if (int.TryParse(groups[2].Value, out iParam))
								{ // check if parameter is an integer
									if (iParam > 0)
									{ // check if parameter is greater than 0
										try
										{ // seeking window for process handle
											iParam = (int)System.Diagnostics.Process.GetProcessById(iParam).MainWindowHandle;
											// process handle converted to window handle and unpin window
											VirtualDesktop.Desktop.UnpinWindow((IntPtr)iParam);
											if (verbose) Console.WriteLine("Window to process id " + groups[2].Value + " unpinned from all desktops");
										}
										catch
										{ // error while seeking
											if (verbose) Console.WriteLine("Window to process id " + groups[2].Value + " not found or unpin failed");
											rc = -1;
										}
									}
									else
										rc = -1;
								}
								else
								{
									try
									{ // seeking window for process name
										iParam = GetMainWindowHandle(groups[2].Value.Trim());
										// unpin window
										VirtualDesktop.Desktop.UnpinWindow((IntPtr)iParam);
										if (verbose) Console.WriteLine("Window of process '" + groups[2].Value + "' unpinned from all desktops");
									}
									catch
									{ // error while seeking
										if (verbose) Console.WriteLine("Process '" + groups[2].Value + "' not found or unpin failed");
										rc = -1;
									}
								}
								break;

							case "UNPINWINDOWHANDLE": // unpin window with handle from all desktops
							case "UPWH":
								if (int.TryParse(groups[2].Value, out iParam))
								{ // check if parameter is an integer
									if (iParam > 0)
									{ // check if parameter is greater than 0
										try
										{ // process window handle and unpin window
											VirtualDesktop.Desktop.UnpinWindow((IntPtr)iParam);
											if (verbose) Console.WriteLine("Window to handle " + groups[2].Value + " unpinned from all desktops");
										}
										catch
										{ // error while seeking
											if (verbose) Console.WriteLine("Window to handle " + groups[2].Value + " not found or unpin failed");
											rc = -1;
										}
									}
									else
										rc = -1;
								}
								else
								{
									try
									{ // seeking window with window title
										iParam = (Int32)GetWindowFromTitle(groups[2].Value.Trim().Replace("^", ""));
										// unpin window
										VirtualDesktop.Desktop.UnpinWindow((IntPtr)iParam);
										if (verbose) Console.WriteLine("Window '" + foundTitle + "' unpinned from all desktops");
									}
									catch
									{ // error while seeking
										if (verbose) Console.WriteLine("Window with text '" + groups[2].Value + "' in title not found or unpin failed");
										rc = -1;
									}
								}
								break;

							case "ISAPPLICATIONPINNED": // is application pinned to all desktops
							case "IAP":
								if (int.TryParse(groups[2].Value, out iParam))
								{ // check if parameter is an integer
									if (iParam > 0)
									{ // check if parameter is greater than 0
										try
										{ // checking desktop for process handle
											iParam = (int)System.Diagnostics.Process.GetProcessById(iParam).MainWindowHandle;
											// process handle converted to window handle
											if (VirtualDesktop.Desktop.IsApplicationPinned((IntPtr)iParam))
											{
												if (verbose) Console.WriteLine("Application to process id " + groups[2].Value + " is pinned to all desktops");
												rc = 0;
											}
											else
											{
												if (verbose) Console.WriteLine("Application to process id " + groups[2].Value + " is not pinned to all desktops");
												rc = 1;
											}
										}
										catch
										{ // error while seeking
											if (verbose) Console.WriteLine("Window to process id " + groups[2].Value + " not found");
											rc = -1;
										}
									}
									else
										rc = -1;
								}
								else
								{
									try
									{ // seeking desktop for process name
										iParam = GetMainWindowHandle(groups[2].Value.Trim());
										if (VirtualDesktop.Desktop.IsApplicationPinned((IntPtr)iParam))
										{
											if (verbose) Console.WriteLine("Application of process '" + groups[2].Value + "' is pinned to all desktops");
											rc = 0;
										}
										else
										{
											if (verbose) Console.WriteLine("Application of process '" + groups[2].Value + "' is not pinned to all desktops");
											rc = 1;
										}
									}
									catch
									{ // error while seeking
										if (verbose) Console.WriteLine("Process '" + groups[2].Value + "' not found");
										rc = -1;
									}
								}
								break;

							case "PINAPPLICATION": // pin application to all desktops
							case "PA":
								if (int.TryParse(groups[2].Value, out iParam))
								{ // check if parameter is an integer
									if (iParam > 0)
									{ // check if parameter is greater than 0
										try
										{ // seeking window for process handle
											iParam = (int)System.Diagnostics.Process.GetProcessById(iParam).MainWindowHandle;
											// process handle converted to window handle and pin window
											VirtualDesktop.Desktop.PinApplication((IntPtr)iParam);
											if (verbose) Console.WriteLine("Application to process id " + groups[2].Value + " pinned to all desktops");
										}
										catch
										{ // error while seeking
											if (verbose) Console.WriteLine("Window to process id " + groups[2].Value + " not found or pin failed");
											rc = -1;
										}
									}
									else
										rc = -1;
								}
								else
								{
									try
									{ // seeking window for process name
										iParam = GetMainWindowHandle(groups[2].Value.Trim());
										// pin window
										VirtualDesktop.Desktop.PinApplication((IntPtr)iParam);
										if (verbose) Console.WriteLine("Application of process '" + groups[2].Value + "' pinned to all desktops");
									}
									catch
									{ // error while seeking
										if (verbose) Console.WriteLine("Process '" + groups[2].Value + "' not found or pin failed");
										rc = -1;
									}
								}
								break;

							case "UNPINAPPLICATION": // unpin application from all desktops
							case "UPA":
								if (int.TryParse(groups[2].Value, out iParam))
								{ // check if parameter is an integer
									if (iParam > 0)
									{ // check if parameter is greater than 0
										try
										{ // seeking window for process handle
											iParam = (int)System.Diagnostics.Process.GetProcessById(iParam).MainWindowHandle;
											// process handle converted to window handle and unpin window
											VirtualDesktop.Desktop.UnpinApplication((IntPtr)iParam);
											if (verbose) Console.WriteLine("Application to process id " + groups[2].Value + " unpinned from all desktops");
										}
										catch
										{ // error while seeking
											if (verbose) Console.WriteLine("Window to process id " + groups[2].Value + " not found or unpin failed");
											rc = -1;
										}
									}
									else
										rc = -1;
								}
								else
								{
									try
									{ // seeking window for process name
										iParam = GetMainWindowHandle(groups[2].Value.Trim());
										// unpin window
										VirtualDesktop.Desktop.UnpinApplication((IntPtr)iParam);
										if (verbose) Console.WriteLine("Application of process '" + groups[2].Value + "' unpinned from all desktops");
									}
									catch
									{ // error while seeking
										if (verbose) Console.WriteLine("Process '" + groups[2].Value + "' not found or unpin failed");
										rc = -1;
									}
								}
								break;

							case "CALCULATE": //calculate
							case "CALC":
							case "CA":
								if (int.TryParse(groups[2].Value, out iParam))
								{ // check if parameter is an integer
									if (verbose) Console.WriteLine("Adding " + iParam.ToString() + " to last result.");
									// adding iParam to result
									rc += iParam;
								}
								else
									rc = -2;
								break;

							case "SLEEP": //wait
							case "SL":
								if (int.TryParse(groups[2].Value, out iParam))
								{ // check if parameter is an integer
									if (iParam > 0)
									{ // check if parameter is greater than 0
										if (verbose) Console.WriteLine("Waiting " + iParam.ToString() + "ms");
										// waiting iParam milliseconds
										System.Threading.Thread.Sleep(iParam);
									}
									else
										rc = -1;
								}
								else
									rc = -2;
								break;

							default:
								rc = -2;
								break;
						}
					}
				}

				if (rc == -1)
				{ // error in action, stop processing
					Console.Error.WriteLine("Error while processing '" + arg + "'");
					if (breakonerror) break;
				}
				if (rc == -2)
				{ // error in parameter, stop processing
					Console.Error.WriteLine("Error in parameter '" + arg + "'");
					if (breakonerror) break;
				}
			}

			return rc;
		}

		static int GetMainWindowHandle(string ProcessName)
		{ // retrieve main window handle to process name
			System.Diagnostics.Process[] processes = System.Diagnostics.Process.GetProcessesByName(ProcessName);
			int wHwnd = 0;

			if (processes.Length > 0)
			{ // process found, get window handle
				wHwnd = (int)processes[0].MainWindowHandle;
			}

			return wHwnd;
		}

		private delegate bool EnumDelegate(IntPtr hWnd, int lParam);

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool IsWindowVisible(IntPtr hWnd);

		[DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
		private static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDelegate lpEnumCallbackFunction, IntPtr lParam);

		[DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
		private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpWindowText, int nMaxCount);

		const int MAXTITLE = 255;
		private static IntPtr foundHandle;
		private static string foundTitle;
		private static string searchTitle;

		private static bool EnumWindowsProc(IntPtr hWnd, int lParam)
		{
			StringBuilder windowText = new StringBuilder(MAXTITLE);
			int titleLength = GetWindowText(hWnd, windowText, windowText.Capacity + 1);
			windowText.Length = titleLength;
			string title = windowText.ToString();

			if (!string.IsNullOrEmpty(title) && IsWindowVisible(hWnd))
			{
				if (title.ToUpper().IndexOf(searchTitle.ToUpper()) >= 0)
				{
					foundHandle = hWnd;
					foundTitle = title;
					return false;
				}
			}
			return true;
		}

		private static IntPtr GetWindowFromTitle(string searchFor)
		{
			searchTitle = searchFor;
			EnumDelegate enumfunc = new EnumDelegate(EnumWindowsProc);

			foundHandle = IntPtr.Zero;
			foundTitle = "";
			EnumDesktopWindows(IntPtr.Zero, enumfunc, IntPtr.Zero);
			if (foundHandle == IntPtr.Zero)
			{
				// Get the last Win32 error code
				int errorCode = Marshal.GetLastWin32Error();
				if (errorCode != 0)
				{ // error
					Console.WriteLine("EnumDesktopWindows failed with code {0}.", errorCode);
				}
			}
			return foundHandle;
		}

		private static int iSwapDesktop1;
		private static int iSwapDesktop2;

		private static bool EnumWindowsProcToSwap(IntPtr hWnd, int lParam)
		{
			StringBuilder windowText = new StringBuilder(MAXTITLE);
			int titleLength = GetWindowText(hWnd, windowText, windowText.Capacity + 1);
			windowText.Length = titleLength;
			string title = windowText.ToString();

			if (!string.IsNullOrEmpty(title) && IsWindowVisible(hWnd))
			{
				try {
					int iDesktopIndex = VirtualDesktop.Desktop.FromDesktop(VirtualDesktop.Desktop.FromWindow(hWnd));
					if (iDesktopIndex == iSwapDesktop1) VirtualDesktop.Desktop.FromIndex(iSwapDesktop2).MoveWindow(hWnd);
					if (iDesktopIndex == iSwapDesktop2) VirtualDesktop.Desktop.FromIndex(iSwapDesktop1).MoveWindow(hWnd);
				}
				catch { }
			}

			return true;
		}

		private static void SwapDesktops(int SwapIndex1, int SwapIndex2)
		{
			iSwapDesktop1 = SwapIndex1;
			iSwapDesktop2 = SwapIndex2;
			EnumDelegate enumfunc = new EnumDelegate(EnumWindowsProcToSwap);

			EnumDesktopWindows(IntPtr.Zero, enumfunc, IntPtr.Zero);
		}

		private static int iInsertDesktop1;
		private static int iInsertDesktop2;

		private static bool EnumWindowsProcToInsert(IntPtr hWnd, int lParam)
		{
			StringBuilder windowText = new StringBuilder(MAXTITLE);
			int titleLength = GetWindowText(hWnd, windowText, windowText.Capacity + 1);
			windowText.Length = titleLength;
			string title = windowText.ToString();

			if (!string.IsNullOrEmpty(title) && IsWindowVisible(hWnd))
			{
				try {
					int iDesktopIndex = VirtualDesktop.Desktop.FromDesktop(VirtualDesktop.Desktop.FromWindow(hWnd));
					if ((iDesktopIndex >= iInsertDesktop1) && (iDesktopIndex < iInsertDesktop2))
						VirtualDesktop.Desktop.FromIndex(iDesktopIndex + 1).MoveWindow(hWnd);

					if (iDesktopIndex == iInsertDesktop2) VirtualDesktop.Desktop.FromIndex(iInsertDesktop1).MoveWindow(hWnd);
				}
				catch { }
			}

			return true;
		}

		private static void InsertDesktop(int InsertIndex1, int InsertIndex2)
		{
			if (InsertIndex2 > InsertIndex1)
			{
				iInsertDesktop1 = InsertIndex1;
				iInsertDesktop2 = InsertIndex2;
			}
			else
			{
				iInsertDesktop1 = InsertIndex2;
				iInsertDesktop2 = InsertIndex1;
			}
			EnumDelegate enumfunc = new EnumDelegate(EnumWindowsProcToInsert);

			EnumDesktopWindows(IntPtr.Zero, enumfunc, IntPtr.Zero);
		}

		static void HelpScreen()
		{
			Console.WriteLine("VirtualDesktop.exe\t\t\t\tMarkus Scholtes, 2021, v1.9\n");

			Console.WriteLine("Command line tool to manage the virtual desktops of Windows 10.");
			Console.WriteLine("Parameters can be given as a sequence of commands. The result - most of the");
			Console.WriteLine("times the number of the processed desktop - can be used as input for the next");
			Console.WriteLine("parameter. The result of the last command is returned as error level.");
			Console.WriteLine("Virtual desktop numbers start with 0.\n");
			Console.WriteLine("Parameters (leading / can be omitted or - can be used instead):\n");
			Console.WriteLine("/Help /h /?      this help screen.");
			Console.WriteLine("/Verbose /Quiet  enable verbose (default) or quiet mode (short: /v and /q).");
			Console.WriteLine("/Break /Continue break (default) or continue on error (short: /b and /co).");
			Console.WriteLine("/List            list all virtual desktops (short: /li).");
			Console.WriteLine("/Count           get count of virtual desktops to pipeline (short: /c).");
			Console.WriteLine("/GetDesktop:<n|s> get number of virtual desktop <n> or desktop with text <s> in");
			Console.WriteLine("                   name to pipeline (short: /gd).");
			Console.WriteLine("/GetCurrentDesktop  get number of current desktop to pipeline (short: /gcd).");
			Console.WriteLine("/IsVisible[:<n|s>] is desktop number <n>, desktop with text <s> in name or with");
			Console.WriteLine("                   number in pipeline visible (short: /iv)? Returns 0 for");
			Console.WriteLine("                   visible and 1 for invisible.");
			Console.WriteLine("/Switch[:<n|s>]  switch to desktop with number <n>, desktop with text <s> in");
			Console.WriteLine("                   name or with number in pipeline (short: /s).");
			Console.WriteLine("/Left            switch to virtual desktop to the left of the active desktop");
			Console.WriteLine("                   (short: /l).");
			Console.WriteLine("/Right           switch to virtual desktop to the right of the active desktop");
			Console.WriteLine("                   (short: /ri).");
			Console.WriteLine("/Wrap /NoWrap    /Left or /Right switch over or generate an error when the edge");
			Console.WriteLine("                   is reached (default)(short /w and /nw).");
			Console.WriteLine("/New             create new desktop (short: /n). Number is stored in pipeline.");
			Console.WriteLine("/Remove[:<n|s>]  remove desktop number <n>, desktop with text <s> in name or");
			Console.WriteLine("                   desktop with number in pipeline (short: /r).");
			Console.WriteLine("/SwapDesktop:<n|s>  swap desktop in pipeline with desktop number <n> or desktop");
			Console.WriteLine("                   with text <s> in name (short: /sd).");
			Console.WriteLine("/InsertDesktop:<n|s>  insert desktop number <n> or desktop with text <s> in");
			Console.WriteLine("                   name before desktop in pipeline or vice versa (short: /id).");
			Console.WriteLine("/MoveWindow:<s|n>  move process with name <s> or id <n> to desktop with number");
			Console.WriteLine("                   in pipeline (short: /mw).");
			Console.WriteLine("/MoveWindowHandle:<s|n>  move window with text <s> in title or handle <n> to");
			Console.WriteLine("                   desktop with number in pipeline (short: /mwh).");
			Console.WriteLine("/MoveActiveWindow  move active window to desktop with number in pipeline");
			Console.WriteLine("                   (short: /maw).");
			Console.WriteLine("/GetDesktopFromWindow:<s|n>  get desktop number where process with name <s> or");
			Console.WriteLine("                   id <n> is displayed (short: /gdfw).");
			Console.WriteLine("/GetDesktopFromWindowHandle:<s|n>  get desktop number where window with text");
			Console.WriteLine("                   <s> in title or handle <n> is displayed (short: /gdfwh).");
			Console.WriteLine("/IsWindowOnDesktop:<s|n>  check if process with name <s> or id <n> is on");
			Console.WriteLine("                   desktop with number in pipeline (short: /iwod). Returns 0");
			Console.WriteLine("                   for yes, 1 for no.");
			Console.WriteLine("/IsWindowHandleOnDesktop:<s|n>  check if window with text <s> in title or");
			Console.WriteLine("                   handle <n> is on desktop with number in pipeline");
			Console.WriteLine("                   (short: /iwhod). Returns 0 for yes, 1 for no.");
			Console.WriteLine("/PinWindow:<s|n>  pin process with name <s> or id <n> to all desktops");
			Console.WriteLine("                   (short: /pw).");
			Console.WriteLine("/PinWindowHandle:<s|n>  pin window with text <s> in title or handle <n> to all");
			Console.WriteLine("                   desktops (short: /pwh).");
			Console.WriteLine("/UnPinWindow:<s|n>  unpin process with name <s> or id <n> from all desktops");
			Console.WriteLine("                   (short: /upw).");
			Console.WriteLine("/UnPinWindowHandle:<s|n>  unpin window with text <s> in title or handle <n>");
			Console.WriteLine("                   from all desktops (short: /upwh).");
			Console.WriteLine("/IsWindowPinned:<s|n>  check if process with name <s> or id <n> is pinned to");
			Console.WriteLine("                   all desktops (short: /iwp). Returns 0 for yes, 1 for no.");
			Console.WriteLine("/IsWindowHandlePinned:<s|n>  check if window with text <s> in title or handle");
			Console.WriteLine("                   <n> is pinned to all desktops (short: /iwhp). Returns 0 for");
			Console.WriteLine("                   yes, 1 for no.");
			Console.WriteLine("/PinApplication:<s|n>  pin application with name <s> or id <n> to all desktops");
			Console.WriteLine("                   (short: /pa).");
			Console.WriteLine("/UnPinApplication:<s|n>  unpin application with name <s> or id <n> from all");
			Console.WriteLine("                   desktops (short: /upa).");
			Console.WriteLine("/IsApplicationPinned:<s|n>  check if application with name <s> or id <n> is");
			Console.WriteLine("                   pinned to all desktops (short: /iap). Returns 0 for yes, 1");
			Console.WriteLine("                   for no.");
			Console.WriteLine("/Calc:<n>        add <n> to result, negative values are allowed (short: /ca).");
			Console.WriteLine("/WaitKey         wait for key press (short: /wk).");
			Console.WriteLine("/Sleep:<n>       wait for <n> milliseconds (short: /sl).\n");
			Console.WriteLine("Hint: Insert ^^ somewhere in window title parameters to prevent finding the own");
			Console.WriteLine("window. ^ is removed before searching window titles.\n");
			Console.WriteLine("Examples:");
			Console.WriteLine("Virtualdesktop.exe /LIST");
			Console.WriteLine("Virtualdesktop.exe \"-Switch:Desktop 2\"");
			Console.WriteLine("Virtualdesktop.exe -New -Switch -GetCurrentDesktop");
			Console.WriteLine("Virtualdesktop.exe Q N /MOVEACTIVEWINDOW /SWITCH");
			Console.WriteLine("Virtualdesktop.exe sleep:200 gd:1 mw:notepad s");
			Console.WriteLine("Virtualdesktop.exe /Count /continue /Remove /Remove /Count");
			Console.WriteLine("Virtualdesktop.exe /Count /Calc:-1 /Switch");
			Console.WriteLine("VirtualDesktop.exe -IsWindowPinned:cmd");
			Console.WriteLine("if ERRORLEVEL 1 VirtualDesktop.exe PinWindow:cmd");
			Console.WriteLine("Virtualdesktop.exe -GetDesktop:1 \"-MoveWindowHandle:note^^pad\"");
		}
	}
}