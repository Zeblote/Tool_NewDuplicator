// * ######################################################################
// *
// *    New Duplicator - Scripts - Server
// *    Pref Manager
// *
// *    -------------------------------------------------------------------
// *    Detect services like RTB to register preferences
// *
// * ######################################################################

//Detect pref service and register preferences
function ndAutoRegisterPrefs(%this)
{
	if($RTB::Hooks::ServerControl)
		ndRegisterRTBPrefs();
	else
		ndExtendDefaultPrefs();
}

//Register preferences to RTB
function ndRegisterRTBPrefs()
{
	echo("ND: Registering RTB prefs");

	%trustDropDown = "list None 0 Build 1 Full 2 Self 3";

	//Limits
	RTB_registerPref("Admin Only",                 "New Duplicator | Limits",   "$Pref::Server::ND::AdminOnly",           "bool",             "Tool_NewDuplicator", false,   false, false, "");
	RTB_registerPref("Fill Paint Admin Only",      "New Duplicator | Limits",   "$Pref::Server::ND::PaintAdminOnly",      "bool",             "Tool_NewDuplicator", false,   false, false, "");
	RTB_registerPref("Fill Paint Fx Admin Only",   "New Duplicator | Limits",   "$Pref::Server::ND::PaintFxAdminOnly",    "bool",             "Tool_NewDuplicator", true,    false, false, "");
	RTB_registerPref("Fill Wrench Admin Only",     "New Duplicator | Limits",   "$Pref::Server::ND::WrenchAdminOnly",     "bool",             "Tool_NewDuplicator", true,    false, false, "");
	RTB_registerPref("Floating Bricks Admin Only", "New Duplicator | Limits",   "$Pref::Server::ND::FloatAdminOnly",      "bool",             "Tool_NewDuplicator", true,    false, false, "");
	RTB_registerPref("Save Admin Only",            "New Duplicator | Limits",   "$Pref::Server::ND::SaveAdminOnly",       "bool",             "Tool_NewDuplicator", true,    false, false, "");
	RTB_registerPref("Load Admin Only",            "New Duplicator | Limits",   "$Pref::Server::ND::LoadAdminOnly",       "bool",             "Tool_NewDuplicator", false,   false, false, "");
	RTB_registerPref("Client Load Admin Only",     "New Duplicator | Limits",   "$Pref::Server::ND::ClientLoadAdminOnly", "bool",             "Tool_NewDuplicator", true,    false, false, "");

	//Settings
	RTB_RegisterPref("Trust Limit",                "New Duplicator | Settings", "$Pref::Server::ND::TrustLimit",          %trustDropDown,     "Tool_NewDuplicator", 2,       false, false, "");
	RTB_RegisterPref("Admin Trust Required",       "New Duplicator | Settings", "$Pref::Server::ND::AdminTrustRequired",  "bool",             "Tool_NewDuplicator", false,   false, false, "");
	RTB_RegisterPref("Select Public Bricks",       "New Duplicator | Settings", "$Pref::Server::ND::SelectPublicBricks",  "bool",             "Tool_NewDuplicator", true,    false, false, "");

	RTB_registerPref("Max Bricks (Admin)",         "New Duplicator | Settings", "$Pref::Server::ND::MaxBricksAdmin",      "int 1000 1000000", "Tool_NewDuplicator", 1000000, false, false, "");
	RTB_registerPref("Max Bricks (Player)",        "New Duplicator | Settings", "$Pref::Server::ND::MaxBricksPlayer",     "int 1000 1000000", "Tool_NewDuplicator", 50000,   false, false, "");
	RTB_registerPref("Max Cube Size (Admin)",      "New Duplicator | Settings", "$Pref::Server::ND::MaxCubeSizeAdmin",    "int 1 50000",      "Tool_NewDuplicator", 256,     false, false, "");
	RTB_registerPref("Max Cube Size (Player)",     "New Duplicator | Settings", "$Pref::Server::ND::MaxCubeSizePlayer",   "int 1 50000",      "Tool_NewDuplicator", 32,      false, false, "");

	RTB_registerPref("Selecting Timeout (Player)", "New Duplicator | Settings", "$Pref::Server::ND::SelectTimeout",       "int 0 20",         "Tool_NewDuplicator", 1,       false, false, "");
	RTB_registerPref("Planting Timeout (Player)",  "New Duplicator | Settings", "$Pref::Server::ND::PlantTimeout",        "int 0 20",         "Tool_NewDuplicator", 1,       false, false, "");

	//Advanced
	RTB_registerPref("Enable Menu Sounds",         "New Duplicator | Advanced", "$Pref::Server::ND::PlayMenuSounds",      "bool",             "Tool_NewDuplicator", true,    false, false, "");
	RTB_registerPref("Highlight Time",             "New Duplicator | Advanced", "$Pref::Server::ND::HighlightDelay",      "int 1 60",         "Tool_NewDuplicator", 8,       false, false, "");
	RTB_registerPref("Max Ghost Bricks",           "New Duplicator | Advanced", "$Pref::Server::ND::MaxGhostBricks",      "int 1 50000",      "Tool_NewDuplicator", 1500,    false, false, "");
	RTB_registerPref("Instant Ghost Bricks",       "New Duplicator | Advanced", "$Pref::Server::ND::InstantGhostBricks",  "int 1 50000",      "Tool_NewDuplicator", 150,     false, false, "");
	RTB_registerPref("Scatter Ghost Bricks",       "New Duplicator | Advanced", "$Pref::Server::ND::ScatterGhostBricks",  "bool",             "Tool_NewDuplicator", true,    false, false, "");
	RTB_registerPref("Process Bricks per Tick",    "New Duplicator | Advanced", "$Pref::Server::ND::ProcessPerTick",      "int 1 50000",      "Tool_NewDuplicator", 300,     false, false, "");
	RTB_registerPref("Cube Selection Chunk Size",  "New Duplicator | Advanced", "$Pref::Server::ND::CubeSelectChunkSize", "int 1 50000",      "Tool_NewDuplicator", 32,      false, false, "");
	RTB_registerPref("Use Old Highlight Method",   "New Duplicator | Advanced", "$Pref::Server::ND::OldHighlightMethod",  "bool",             "Tool_NewDuplicator", false,   false, false, "");

	//Restore default prefs
	RTB_registerPref("Check to restore defaults", "New Duplicator | Reset Prefs", "$ND::RestoreDefaultPrefs", "bool", "Tool_NewDuplicator", false, false, false, "ndResetPrefs");
}

//Set default values, if they haven't been set already
function ndExtendDefaultPrefs()
{
	echo("ND: Extending default prefs");

	//Limits
	if($Pref::Server::ND::AdminOnly           $= "") $Pref::Server::ND::AdminOnly           = false;
	if($Pref::Server::ND::PaintAdminOnly      $= "") $Pref::Server::ND::PaintAdminOnly      = false;
	if($Pref::Server::ND::PaintFxAdminOnly    $= "") $Pref::Server::ND::PaintFxAdminOnly    = true;
	if($Pref::Server::ND::WrenchAdminOnly     $= "") $Pref::Server::ND::WrenchAdminOnly     = true;
	if($Pref::Server::ND::FloatAdminOnly      $= "") $Pref::Server::ND::FloatAdminOnly      = true;
	if($Pref::Server::ND::SaveAdminOnly       $= "") $Pref::Server::ND::SaveAdminOnly       = true;
	if($Pref::Server::ND::LoadAdminOnly       $= "") $Pref::Server::ND::LoadAdminOnly       = false;
	if($Pref::Server::ND::ClientLoadAdminOnly $= "") $Pref::Server::ND::ClientLoadAdminOnly = true;

	//Settings
	if($Pref::Server::ND::TrustLimit          $= "") $Pref::Server::ND::TrustLimit          = 2;
	if($Pref::Server::ND::AdminTrustRequired  $= "") $Pref::Server::ND::AdminTrustRequired  = false;
	if($Pref::Server::ND::SelectPublicBricks  $= "") $Pref::Server::ND::SelectPublicBricks  = true;

	if($Pref::Server::ND::MaxBricksAdmin      $= "") $Pref::Server::ND::MaxBricksAdmin      = 1000000;
	if($Pref::Server::ND::MaxBricksPlayer     $= "") $Pref::Server::ND::MaxBricksPlayer     = 10000;
	if($Pref::Server::ND::MaxCubeSizeAdmin    $= "") $Pref::Server::ND::MaxCubeSizeAdmin    = 256;
	if($Pref::Server::ND::MaxCubeSizePlayer   $= "") $Pref::Server::ND::MaxCubeSizePlayer   = 32;

	if($Pref::Server::ND::SelectTimeout       $= "") $Pref::Server::ND::SelectTimeout       = 1;
	if($Pref::Server::ND::PlantTimeout        $= "") $Pref::Server::ND::PlantTimeout        = 1;

	//Advanced
	if($Pref::Server::ND::PlayMenuSounds      $= "") $Pref::Server::ND::PlayMenuSounds      = true;
	if($Pref::Server::ND::HighlightDelay      $= "") $Pref::Server::ND::HighlightDelay      = 8;
	if($Pref::Server::ND::MaxGhostBricks      $= "") $Pref::Server::ND::MaxGhostBricks      = 1500;
	if($Pref::Server::ND::InstantGhostBricks  $= "") $Pref::Server::ND::InstantGhostBricks  = 150;
	if($Pref::Server::ND::ScatterGhostBricks  $= "") $Pref::Server::ND::ScatterGhostBricks  = true;
	if($Pref::Server::ND::ProcessPerTick      $= "") $Pref::Server::ND::ProcessPerTick      = 300;
	if($Pref::Server::ND::CubeSelectChunkSize $= "") $Pref::Server::ND::CubeSelectChunkSize = 32;
	if($Pref::Server::ND::OldHighlightMethod  $= "") $Pref::Server::ND::OldHighlightMethod  = false;

	//Always set this to false
	$ND::RestoreDefaultPrefs = false;
}

//Set default values
function ndApplyDefaultPrefs(%this)
{
	echo("ND: Applying default prefs");
	messageAll('', "\c6(\c3New Duplicator\c6) \c6Prefs reset to default values.");

	//Limits
	$Pref::Server::ND::AdminOnly           = false;
	$Pref::Server::ND::PaintAdminOnly      = false;
	$Pref::Server::ND::PaintFxAdminOnly    = true;
	$Pref::Server::ND::WrenchAdminOnly     = true;
	$Pref::Server::ND::FloatAdminOnly      = true;
	$Pref::Server::ND::SaveAdminOnly       = true;
	$Pref::Server::ND::LoadAdminOnly       = false;
	$Pref::Server::ND::ClientLoadAdminOnly = true;

	//Settings
	$Pref::Server::ND::TrustLimit          = 2;
	$Pref::Server::ND::AdminTrustRequired  = false;
	$Pref::Server::ND::SelectPublicBricks  = true;

	$Pref::Server::ND::MaxBricksAdmin      = 1000000;
	$Pref::Server::ND::MaxBricksPlayer     = 10000;
	$Pref::Server::ND::MaxCubeSizeAdmin    = 256;
	$Pref::Server::ND::MaxCubeSizePlayer   = 32;

	$Pref::Server::ND::SelectTimeout       = 1;
	$Pref::Server::ND::PlantTimeout        = 1;

	//Advanced
	$Pref::Server::ND::PlayMenuSounds      = true;
	$Pref::Server::ND::HighlightDelay      = 8;
	$Pref::Server::ND::MaxGhostBricks      = 1500;
	$Pref::Server::ND::InstantGhostBricks  = 150;
	$Pref::Server::ND::ScatterGhostBricks  = true;
	$Pref::Server::ND::ProcessPerTick      = 300;
	$Pref::Server::ND::CubeSelectChunkSize = 32;
	$Pref::Server::ND::OldHighlightMethod  = false;

	//Always set this to false
	$ND::RestoreDefaultPrefs = false;
}

//Callback function to restore default prefs
function ndResetPrefs()
{
	if($ND::RestoreDefaultPrefs)
		ndApplyDefaultPrefs();
}
