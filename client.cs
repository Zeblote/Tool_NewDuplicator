// * ######################################################################
// *
// *    New Duplicator - Client
// *    Main Script
// *
// *    -------------------------------------------------------------------
// *    Executes required classes and scripts, initializes client side
// *
// * ######################################################################

$ND::Version = "1.5.0";

$ND::FilePath = filePath($Con::File) @ "/";
$ND::ConfigPath = "config/NewDuplicator/";

$ND::ClassPath = $ND::FilePath @ "classes/";
$ND::ScriptPath = $ND::FilePath @ "scripts/";
$ND::ResourcePath = $ND::FilePath @ "resources/";

echo(" \n--- Loading Client Guis ---");
exec($ND::ScriptPath @ "client/guis/fillwrench.gui");

echo(" \n--- Loading Client Scripts ---");
exec($ND::ScriptPath @ "client/controls.cs");
exec($ND::ScriptPath @ "client/handshake.cs");
exec($ND::ScriptPath @ "client/wrench.cs");

if(!$Pref::ND::DisableUpdater
	&& !$SupportUpdaterMigration
	&& !isFile("Add-Ons/Support_Updater.zip"))
{
	exec($ND::ScriptPath @ "client/tcpclient.cs");
	exec($ND::ScriptPath @ "client/updater.cs");
}

echo(" \n--- Initializing Client ---");
activatePackage(NewDuplicator_Client);
ndRegisterKeybinds();
