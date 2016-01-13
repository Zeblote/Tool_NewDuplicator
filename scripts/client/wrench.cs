// * ######################################################################
// *
// *    New Duplicator - Client
// *    Wrench
// *
// *    -------------------------------------------------------------------
// *    Prepares wrench gui for fill wrench mode
// *
// * ######################################################################

package NewDuplicator_Client
{
	function clientCmdWrench_LoadMenus()
	{
		parent::clientCmdWrench_LoadMenus();

		$ND::WrenchReloadRequired = true;
	}
};

//Open the wrench gui for fill wrench mode
function clientCmdNdOpenWrenchGui()
{
	if($ND::WrenchReloadRequired)
	{
		//Reload the drop down lists
		ND_Wrench_Lights.clear();
		ND_Wrench_Emitters.clear();
		ND_Wrench_Items.clear();

		ND_Wrench_Lights.add(" NONE", 0);
		ND_Wrench_Emitters.add(" NONE", 0);
		ND_Wrench_Items.add(" NONE", 0);

		//Add all datablocks to list
		%cnt = getDatablockGroupSize();

		for(%i = 0; %i < %cnt; %i++)
		{
			%data = getDatablock(%i);
			%uiName = %data.uiName;

			//Skip non-selectable datablocks
			if(%uiName $= "")
				continue;

			//Put datablock in correct list
			switch$(%data.getClassName())
			{
				case "FxLightData":
					ND_Wrench_Lights.add(%uiName, %data);

				case "ParticleEmitterData":
					ND_Wrench_Emitters.add(%uiName, %data);

				case "ItemData":
					ND_Wrench_Items.add(%uiName, %data);
			}
		}

		//Sort lists
		ND_Wrench_Lights.sort();
		ND_Wrench_Emitters.sort();
		ND_Wrench_Items.sort();

		//Select NONE
		ND_Wrench_Lights.setSelected(0);
		ND_Wrench_Emitters.setSelected(0);
		ND_Wrench_Items.setSelected(0);

		$ND::WrenchReloadRequired = false;
	}

	//Open gui
	Canvas.pushDialog(ND_WrenchDlg);
}

//Send the settings to the server
function ndSendFillWrenchData()
{
	//Close gui
	Canvas.popDialog(ND_WrenchDlg);

	//Pack all enabled settings in string
	%str = "";

	if(ND_Wrench_ToggleName.getValue())
		%str = %str TAB "N" SPC trim(ND_Wrench_Name.getValue());

	if(ND_Wrench_ToggleLights.getValue())
		%str = %str TAB "LDB" SPC ND_Wrench_Lights.getSelected();

	if(ND_Wrench_ToggleEmitters.getValue())
		%str = %str TAB "EDB" SPC ND_Wrench_Emitters.getSelected();

	if(ND_Wrench_ToggleEmitterDir.getValue())
	{
		%dir = -1;

		for(%i = 0; %i < 6; %i++)
		{
			%obj = "ND_Wrench_EmitterDir" @ %i;

			if(%obj.getValue())
			{
				%dir = %i;
				break;
			}
		}

		if(%dir >= 0)
			%str = %str TAB "EDIR" SPC %dir;
	}

	if(ND_Wrench_ToggleItems.getValue())
		%str = %str TAB "IDB" SPC ND_Wrench_Items.getSelected();

	if(ND_Wrench_ToggleItemPos.getValue())
	{
		%pos = -1;

		for(%i = 0; %i < 6; %i++)
		{
			%obj = "ND_Wrench_ItemPos" @ %i;

			if(%obj.getValue())
			{
				%pos = %i;
				break;
			}
		}

		if(%pos >= 0)
			%str = %str TAB "IPOS" SPC %pos;
	}

	if(ND_Wrench_ToggleItemDir.getValue())
	{
		%dir = -1;

		for(%i = 2; %i < 6; %i++)
		{
			%obj = "ND_Wrench_ItemDir" @ %i;

			if(%obj.getValue())
			{
				%dir = %i;
				break;
			}
		}

		if(%dir >= 2)
			%str = %str TAB "IDIR" SPC %dir;
	}

	if(ND_Wrench_ToggleItemTime.getValue())
		%str = %str TAB "IRT" SPC trim(ND_Wrench_ItemTime.getValue()) * 1;

	if(ND_Wrench_ToggleRayCasting.getValue())
		%str = %str TAB "RC" SPC ND_Wrench_RayCasting.getValue();

	if(ND_Wrench_ToggleCollision.getValue())
		%str = %str TAB "C" SPC ND_Wrench_Collision.getValue();

	if(ND_Wrench_ToggleRendering.getValue())
		%str = %str TAB "R" SPC ND_Wrench_Rendering.getValue();

	//Send string
	if(strLen(%str))
		commandToServer('ndStartFillWrench', trim(%str));
}
