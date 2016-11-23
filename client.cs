// Executes all required scripts and initializes the client side.
// -------------------------------------------------------------------

$ND::Version = "1.5.2";

$ND::FilePath = filePath($Con::File) @ "/";
$ND::ConfigPath = "config/NewDuplicator/";

$ND::ClassPath = $ND::FilePath @ "classes/";
$ND::ScriptPath = $ND::FilePath @ "scripts/";
$ND::ResourcePath = $ND::FilePath @ "resources/";

exec($ND::ScriptPath @ "client/guis/fillwrench.gui");

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

activatePackage(NewDuplicator_Client);
ndRegisterKeybinds();
