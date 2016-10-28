// Installs a copy of Support_Updater to handle automatic updates.
// Based on http://forum.blockland.us/index.php?topic=255382.0
// -------------------------------------------------------------------

function ndInstallUpdaterPrompt()
{
	%message = "<just:left>The New Duplicator is regularly updated with new modes, features, and bug fixes."
	@ " To make this much easier for you, an automatic updater is available! (Support_Updater by Greek2Me)"
	NL "\nJust click yes below to install it in the background. Click no to be reminded later.";

	messageBoxYesNo("New Duplicator | Automatic Updates", %message, "ndInstallUpdater();");
}

function ndInstallUpdater()
{
	%url = "http://mods.greek2me.us/storage/Support_Updater.zip";
	%downloadPath = "Add-Ons/Support_Updater.zip";
	%className = "ND_InstallUpdaterTCP";

	connectToURL(%url, "GET", %downloadPath, %className);
	messageBoxOK("New Duplicator | Downloading Updater", "Trying to download the updater...");
}

function ND_InstallUpdaterTCP::onDone(%this, %error)
{
	if(%error)
		messageBoxOK("New Duplicator | Error :(", "Error downloading the updater:" NL %error NL "You'll be prompted again at a later time.");
	else
	{
		messageBoxOK("New Duplicator | Updater Installed", "The updater has been installed.\n\nHave fun!");

		discoverFile("Add-ons/Support_Updater.zip");
		exec("Add-ons/Support_Updater/client.cs");
	}
}

schedule(1000, 0, "ndInstallUpdaterPrompt");
$SupportUpdaterMigration = true;
