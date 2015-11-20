// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    ND_PrefManager
// *
// *    -------------------------------------------------------------------
// *    Detect services like RTB to register preferences
// *
// * ######################################################################

//Create pref manager
function ND_PrefManager()
{
	ND_ServerGroup.add(
		%this = new ScriptObject(ND_PrefManager)
	);

	return %this;
}

//Detect pref service and register preferences
function ND_PrefManager::registerPrefs(%this)
{
	if($RTB::Hooks::ServerControl)
		%this.registerRTBPrefs();
	else
		%this.setDefaultValues();
}

//Register preferences to RTB
function ND_PrefManager::registerRTBPrefs(%this)
{
	echo("ND_PrefManager registering RTB prefs");

	//General
	RTB_registerPref("Admin Only",                  "New Duplicator | General",     "$Pref::Server::ND::AdminOnly",               "bool",             "Tool_NewDuplicator", false,   false, false, "");
	RTB_registerPref("Max Bricks (Admin)",          "New Duplicator | General",     "$Pref::Server::ND::MaxBricksAdmin",          "int 1000 1000000", "Tool_NewDuplicator", 1000000, false, false, "");
	RTB_registerPref("Max Bricks (Player)",         "New Duplicator | General",     "$Pref::Server::ND::MaxBricksPlayer",         "int 1000 1000000", "Tool_NewDuplicator", 50000,   false, false, "");
	RTB_registerPref("Max Cube Size (Admin)",       "New Duplicator | General",     "$Pref::Server::ND::MaxCubeSizeAdmin",        "int 1 50000",      "Tool_NewDuplicator", 1024,    false, false, "");
	RTB_registerPref("Max Cube Size (Player)",      "New Duplicator | General",     "$Pref::Server::ND::MaxCubeSizePlayer",       "int 1 50000",      "Tool_NewDuplicator", 32,      false, false, "");
	RTB_registerPref("Enable Menu Sounds",          "New Duplicator | General",     "$Pref::Server::ND::PlayMenuSounds",          "bool",             "Tool_NewDuplicator", true,    false, false, "");
	RTB_registerPref("Advertise New Duplicator",    "New Duplicator | General",     "$Pref::Server::ND::Advertise",               "bool",             "Tool_NewDuplicator", true,    false, false, "");

	//Colors
	RTB_registerPref("Highlight Color Id",          "New Duplicator | Colors",      "$Pref::Server::ND::BrickHighlightColor",     "int 0 63",         "Tool_NewDuplicator", 3,       false, false, "");
	RTB_registerPref("Highlight Color Fx Id",       "New Duplicator | Colors",      "$Pref::Server::ND::BrickHighlightColorFx",   "int 0 6",          "Tool_NewDuplicator", 3,       false, false, "");

	//Highlight
	RTB_registerPref("Highlight Time (ms)",         "New Duplicator | Highlight",   "$Pref::Server::ND::HighlightTime",           "int 0 50000",      "Tool_NewDuplicator", 8000,    false, false, "");
	RTB_registerPref("De-Highlight Tick (ms)",      "New Duplicator | Highlight",   "$Pref::Server::ND::DeHighlightTickDelay",    "int 1 50000",      "Tool_NewDuplicator", 30,      false, false, "");
	RTB_registerPref("De-Highlight per Tick",       "New Duplicator | Highlight",   "$Pref::Server::ND::DeHighlightPerTick",      "int 1 50000",      "Tool_NewDuplicator", 300,     false, false, "");

	//Selection
	RTB_registerPref("Stack Select Tick (ms)",      "New Duplicator | Selection",   "$Pref::Server::ND::StackSelectTickDelay",    "int 1 50000",      "Tool_NewDuplicator", 30,      false, false, "");
	RTB_registerPref("Stack Select per Tick",       "New Duplicator | Selection",   "$Pref::Server::ND::StackSelectPerTick",      "int 1 50000",      "Tool_NewDuplicator", 300,     false, false, "");

	RTB_registerPref("Cube Select Tick (ms)",       "New Duplicator | Selection",   "$Pref::Server::ND::CubeSelectTickDelay",     "int 1 50000",      "Tool_NewDuplicator", 30,      false, false, "");
	RTB_registerPref("Cube Select per Tick",        "New Duplicator | Selection",   "$Pref::Server::ND::CubeSelectPerTick",       "int 1 50000",      "Tool_NewDuplicator", 300,    false, false, "");
	RTB_registerPref("Cube Select Chunk Size",      "New Duplicator | Selection",   "$Pref::Server::ND::CubeSelectChunkSize",     "int 1 50000",      "Tool_NewDuplicator", 32,      false, false, "");

	//Ghosting
	RTB_registerPref("Scatter Ghost Bricks",        "New Duplicator | Ghosting",    "$Pref::Server::ND::ScatterGhostBricks",      "bool",             "Tool_NewDuplicator", true,    false, false, "");

	RTB_registerPref("Max Ghost Bricks",            "New Duplicator | Ghosting",    "$Pref::Server::ND::MaxGhostBricks",          "int 1 50000",      "Tool_NewDuplicator", 1500,    false, false, "");
	RTB_registerPref("Instant Ghost Bricks",        "New Duplicator | Ghosting",    "$Pref::Server::ND::InstantGhostBricks",      "int 1 50000",      "Tool_NewDuplicator", 150,     false, false, "");

	RTB_registerPref("Instant Ghost Delay (ms)",    "New Duplicator | Ghosting",    "$Pref::Server::ND::GhostBricksInitialDelay", "int 1 50000",      "Tool_NewDuplicator", 400,     false, false, "");
	RTB_registerPref("Move Ghost Bricks Tick (ms)", "New Duplicator | Ghosting",    "$Pref::Server::ND::GhostBricksTickDelay",    "int 1 50000",      "Tool_NewDuplicator", 30,      false, false, "");
	RTB_registerPref("Move Ghost Bricks per Tick",  "New Duplicator | Ghosting",    "$Pref::Server::ND::GhostBricksPerTick",      "int 1 50000",      "Tool_NewDuplicator", 300,     false, false, "");

	//Planting
	RTB_registerPref("Plant Bricks Tick (ms)",      "New Duplicator | Planting",    "$Pref::Server::ND::PlantBricksTickDelay",    "int 1 50000",      "Tool_NewDuplicator", 30,      false, false, "");
	RTB_registerPref("Plant Bricks per Tick",       "New Duplicator | Planting",    "$Pref::Server::ND::PlantBricksPerTick",      "int 1 50000",      "Tool_NewDuplicator", 300,     false, false, "");

	//Restore default prefs
	RTB_registerPref("Check to restore defaults",   "New Duplicator | Reset Prefs", "$ND::RestoreDefaultPrefs",                   "bool",             "Tool_NewDuplicator", false,   false, false, "ndResetPrefs");
}

//Set default values
function ND_PrefManager::setDefaultValues(%this)
{
	echo("ND_PrefManager setting default prefs");

	//General
	$Pref::Server::ND::AdminOnly               = false;
	$Pref::Server::ND::MaxBricksAdmin          = 1000000;
	$Pref::Server::ND::MaxBricksPlayer         = 50000;
	$Pref::Server::ND::MaxCubeSizeAdmin        = 1024;
	$Pref::Server::ND::MaxCubeSizePlayer       = 32;
	$Pref::Server::ND::PlayMenuSounds          = true;
	$Pref::Server::ND::Advertise               = true;

	//Colors
	$Pref::Server::ND::BrickHighlightColor     = 3;
	$Pref::Server::ND::BrickHighlightColorFx   = 3;

	//Highlight
	$Pref::Server::ND::HighlightTime           = 8000;
	$Pref::Server::ND::DeHighlightTickDelay    = 30;
	$Pref::Server::ND::DeHighlightPerTick      = 300;

	//Selection
	$Pref::Server::ND::StackSelectTickDelay    = 30;
	$Pref::Server::ND::StackSelectPerTick      = 300;

	$Pref::Server::ND::CubeSelectTickDelay     = 30;
	$Pref::Server::ND::CubeSelectPerTick       = 300;
	$Pref::Server::ND::CubeSelectChunkSize     = 32;

	//Ghosting
	$Pref::Server::ND::ScatterGhostBricks      = true;

	$Pref::Server::ND::MaxGhostBricks          = 1500;
	$Pref::Server::ND::InstantGhostBricks      = 150;

	$Pref::Server::ND::GhostBricksInitialDelay = 400;
	$Pref::Server::ND::GhostBricksTickDelay    = 30;
	$Pref::Server::ND::GhostBricksPerTick      = 300;

	//Planting
	$Pref::Server::ND::PlantBricksTickDelay    = 30;
	$Pref::Server::ND::PlantBricksPerTick      = 300;

	//Always set this to false
	$ND::RestoreDefaultPrefs = false;
}

//Print prefs to client (debug, may be useful for release?)
function ND_PrefManager::dumpPrefs(%this, %client)
{
	messageClient(%client, '', "\c6New Duplicator pref values");	
	messageClient(%client, '', "\c7General");
	messageClient(%client, '', "\c6      Admin Only: \c3" @ ($Pref::Server::ND::AdminOnly ? "Y" : "N"));
	messageClient(%client, '', "\c6      Max Bricks (Admin): \c3" @ $Pref::Server::ND::MaxBricksAdmin);
	messageClient(%client, '', "\c6      Max Bricks (Player): \c3" @ $Pref::Server::ND::MaxBricksPlayer);
	messageClient(%client, '', "\c6      Max Cube Size (Admin): \c3" @ $Pref::Server::ND::MaxCubeSizeAdmin);
	messageClient(%client, '', "\c6      Max Cube Size (Player): \c3" @ $Pref::Server::ND::MaxCubeSizePlayer);
	messageClient(%client, '', "\c6      Enable Menu Sounds: \c3" @ ($Pref::Server::ND::PlayMenuSounds ? "Y" : "N"));
	messageClient(%client, '', "\c6      Advertise New Duplicator: \c3" @ ($Pref::Server::ND::Advertise ? "Y" : "N"));

	messageClient(%client, '', "\c7Colors");
	messageClient(%client, '', "\c6      Highlight Color: \c3" @ $Pref::Server::ND::BrickHighlightColor);
	messageClient(%client, '', "\c6      Highlight Color Fx: \c3" @ $Pref::Server::ND::BrickHighlightColorFx);

	messageClient(%client, '', "\c7Highlight");
	messageClient(%client, '', "\c6      Highlight Time: \c3" @ $Pref::Server::ND::HighlightTime);
	messageClient(%client, '', "\c6      De-Highlight Tick (ms): \c3" @ $Pref::Server::ND::DeHighlightTickDelay);
	messageClient(%client, '', "\c6      De-Highlight per Tick: \c3" @ $Pref::Server::ND::DeHighlightPerTick);

	messageClient(%client, '', "\c7Selection");
	messageClient(%client, '', "\c6      Stack Select Tick (ms): \c3" @ $Pref::Server::ND::StackSelectTickDelay);
	messageClient(%client, '', "\c6      Stack Select per Tick: \c3" @ $Pref::Server::ND::StackSelectPerTick);
	messageClient(%client, '', "\c6      Cube Select Tick (ms): \c3" @ $Pref::Server::ND::CubeSelectTickDelay);
	messageClient(%client, '', "\c6      Cube Select per Tick: \c3" @ $Pref::Server::ND::CubeSelectPerTick);
	messageClient(%client, '', "\c6      Cube Select Chunk Size: \c3" @ $Pref::Server::ND::CubeSelectChunkSize);

	messageClient(%client, '', "\c7Ghosting");
	messageClient(%client, '', "\c6      Scatter Ghost Bricks: \c3" @ ($Pref::Server::ND::ScatterGhostBricks ? "Y" : "N"));
	messageClient(%client, '', "\c6      Max Ghost Bricks: \c3" @ $Pref::Server::ND::MaxGhostBricks);
	messageClient(%client, '', "\c6      Instant Ghost Bricks: \c3" @ $Pref::Server::ND::InstantGhostBricks);
	messageClient(%client, '', "\c6      Instant Ghost Delay (ms): \c3" @ $Pref::Server::ND::GhostBricksInitialDelay);
	messageClient(%client, '', "\c6      Move Ghost Bricks Tick (ms): \c3" @ $Pref::Server::ND::GhostBricksTickDelay);
	messageClient(%client, '', "\c6      Move Ghost Bricks per Tick: \c3" @ $Pref::Server::ND::GhostBricksPerTick);

	messageClient(%client, '', "\c7Planting");
	messageClient(%client, '', "\c6      Plant Bricks Tick (ms): \c3" @ $Pref::Server::ND::PlantBricksTickDelay);
	messageClient(%client, '', "\c6      Plant Bricks per Tick: \c3" @ $Pref::Server::ND::PlantBricksPerTick);
}

//Callback function to restore default prefs
function ndResetPrefs()
{
	if($ND::RestoreDefaultPrefs)
		ND_PrefManager.setDefaultValues();
}
