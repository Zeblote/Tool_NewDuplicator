// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    ND_SelectionBox
// *
// *    -------------------------------------------------------------------
// *    Resizable and recolorable box using 14 static shapes
// *
// * ######################################################################

//Create a new selection box
function ND_SelectionBox(%shapeName)
{
	ND_ServerGroup.add(
		%this = new ScriptObject(ND_SelectionBox)
	);

	%this.innerCube = new StaticShape(){datablock = ND_SelectionCubeInnerDts;};
	%this.outerCube = new StaticShape(){datablock = ND_SelectionCubeOuterDts;};
	%this.shapeName = new StaticShape(){datablock = ND_ShapeNameDts;};

	for(%i = 0; %i < 4; %i++)
	{
		%this.border_x[%i] = new StaticShape(){datablock = ND_SelectionCubeBorderDts;};
		%this.border_y[%i] = new StaticShape(){datablock = ND_SelectionCubeBorderDts;};
		%this.border_z[%i] = new StaticShape(){datablock = ND_SelectionCubeBorderDts;};
	}

	%this.innerColor = "0 0 0 0.85";
	%this.outerColor = "1 0.84 0 0.4";
	%this.borderColor = "1 0.84 0 1";

	%this.innerColorSelected = "1 0 0 0.85";
	%this.outerColorSelected = "1 0 0 0.4";
	%this.borderColorSelected = "1 0 0 1";

	%this.shapeName.setShapeNameColor(%this.borderColor);
	%this.shapeName.setShapeName(%shapeName);

	%this.recolor();

	return %this;
}

//Destroy static shapes when selection box is removed
function ND_SelectionBox::onRemove(%this)
{
	%this.innerCube.delete();
	%this.outerCube.delete();
	%this.shapeName.delete();

	for(%i = 0; %i < 4; %i++)
	{
		%this.border_x[%i].delete();
		%this.border_y[%i].delete();
		%this.border_z[%i].delete();
	}
}

//Apply color changes to the selection box
function ND_SelectionBox::recolor(%this)
{
	%this.innerCube.setNodeColor("ALL", %this.innerColor);
	%this.outerCube.setNodeColor("ALL", %this.outerColor);

	for(%i = 0; %i < 4; %i++)
	{
		%this.border_x[%i].setNodeColor("border", %this.borderColor);
		%this.border_y[%i].setNodeColor("border", %this.borderColor);
		%this.border_z[%i].setNodeColor("border", %this.borderColor);
	}

	if(strLen(%this.selectedSide))
	{
		%this.innerCube.setNodeColor("in" @ %this.selectedSide, %this.innerColorSelected);
		%this.outerCube.setNodeColor("out" @ %this.selectedSide, %this.outerColorSelected);	

		switch$(%this.selectedSide)
		{
		case "+X":
			%this.border_y1.setNodeColor("border", %this.borderColorSelected);
			%this.border_y2.setNodeColor("border", %this.borderColorSelected);
			%this.border_z1.setNodeColor("border", %this.borderColorSelected);
			%this.border_z2.setNodeColor("border", %this.borderColorSelected);

		case "-X":
			%this.border_y0.setNodeColor("border", %this.borderColorSelected);
			%this.border_y3.setNodeColor("border", %this.borderColorSelected);
			%this.border_z0.setNodeColor("border", %this.borderColorSelected);
			%this.border_z3.setNodeColor("border", %this.borderColorSelected);

		case "+Y":
			%this.border_x1.setNodeColor("border", %this.borderColorSelected);
			%this.border_x2.setNodeColor("border", %this.borderColorSelected);
			%this.border_z2.setNodeColor("border", %this.borderColorSelected);
			%this.border_z3.setNodeColor("border", %this.borderColorSelected);

		case "-Y":
			%this.border_x0.setNodeColor("border", %this.borderColorSelected);
			%this.border_x3.setNodeColor("border", %this.borderColorSelected);
			%this.border_z0.setNodeColor("border", %this.borderColorSelected);
			%this.border_z1.setNodeColor("border", %this.borderColorSelected);

		case "+Z":
			%this.border_x2.setNodeColor("border", %this.borderColorSelected);
			%this.border_x3.setNodeColor("border", %this.borderColorSelected);
			%this.border_y2.setNodeColor("border", %this.borderColorSelected);
			%this.border_y3.setNodeColor("border", %this.borderColorSelected);

		case "-Z":
			%this.border_x0.setNodeColor("border", %this.borderColorSelected);
			%this.border_x1.setNodeColor("border", %this.borderColorSelected);
			%this.border_y0.setNodeColor("border", %this.borderColorSelected);
			%this.border_y1.setNodeColor("border", %this.borderColorSelected);
		}
	}
}

//Return current size of selection box
function ND_SelectionBox::getSize(%this)
{
	return %this.point1 SPC %this.point2;
}

//Resize the selection box
function ND_SelectionBox::resize(%this, %point1, %point2)
{
	if(getWordCount(%point1) == 6)
	{
		%point2 = getWords(%point1, 3, 5);
		%point1 = getWords(%point1, 0, 2);
	}

	%this.point1 = %point1;
	%this.point2 = %point2;

	%x1 = getWord(%point1, 0);
	%y1 = getWord(%point1, 1);
	%z1 = getWord(%point1, 2);

	%x2 = getWord(%point2, 0);
	%y2 = getWord(%point2, 1);
	%z2 = getWord(%point2, 2);

	%len_x = %x2 - %x1;
	%len_y = %y2 - %y1;
	%len_z = %z2 - %z1;

	%center_x = (%x1 + %x2) / 2;
	%center_y = (%y1 + %y2) / 2;
	%center_z = (%z1 + %z2) / 2;

	%rot_x = "0 1 0 1.57079";
	%rot_y = "1 0 0 1.57079";
	%rot_z = "0 0 1 0";

	%this.innerCube.setTransform(%center_x SPC %center_y SPC %center_z);
	%this.outerCube.setTransform(%center_x SPC %center_y SPC %center_z);
	%this.shapeName.setTransform(%center_X SPC %center_y SPC %z2);

	%this.border_x0.setTransform(%center_x SPC %y1 SPC %z1 SPC %rot_x);
	%this.border_x1.setTransform(%center_x SPC %y2 SPC %z1 SPC %rot_x);
	%this.border_x2.setTransform(%center_x SPC %y2 SPC %z2 SPC %rot_x);
	%this.border_x3.setTransform(%center_x SPC %y1 SPC %z2 SPC %rot_x);

	%this.border_y0.setTransform(%x1 SPC %center_y SPC %z1 SPC %rot_y);
	%this.border_y1.setTransform(%x2 SPC %center_y SPC %z1 SPC %rot_y);
	%this.border_y2.setTransform(%x2 SPC %center_y SPC %z2 SPC %rot_y);
	%this.border_y3.setTransform(%x1 SPC %center_y SPC %z2 SPC %rot_y);

	%this.border_z0.setTransform(%x1 SPC %y1 SPC %center_z SPC %rot_z);
	%this.border_z1.setTransform(%x2 SPC %y1 SPC %center_z SPC %rot_z);
	%this.border_z2.setTransform(%x2 SPC %y2 SPC %center_z SPC %rot_z);
	%this.border_z3.setTransform(%x1 SPC %y2 SPC %center_z SPC %rot_z);

	%this.innerCube.setScale(%len_x - 0.02 SPC %len_y - 0.02 SPC %len_z - 0.02);
	%this.outerCube.setScale(%len_x + 0.02 SPC %len_y + 0.02 SPC %len_z + 0.02);


	%maxLen = %len_y > %len_x ? %len_y : %len_x;
	%maxLen = %len_z > %maxLen ? %len_z : %maxLen;

	if(%maxLen > 1024.1)
		%width = 7;
	else if(%maxLen > 512.1)
		%width = 6;
	else if(%maxLen > 256.1)
		%width = 5;
	else if(%maxLen > 128.1)
		%width = 4;
	else if(%maxLen > 64.1)
		%width = 3;
	else if(%maxLen > 32.1)
		%width = 2;
	else if(%maxLen > 4.1)
		%width = 1;
	else
		%width = 0.5;

	for(%i = 0; %i < 4; %i++)
	{
		%this.border_x[%i].setScale(%width SPC %width SPC %len_x + %width * 0.05);
		%this.border_y[%i].setScale(%width SPC %width SPC %len_y + %width * 0.05);
		%this.border_z[%i].setScale(%width SPC %width SPC %len_z + %width * 0.05);
	}
}

//Select a side of the box
function ND_SelectionBox::selectSide(%this, %x, %y, %z)
{
	%old = %this.selectedSide;

	if(%x > 0)
		%side = "+X";
	else if(%x < 0)
		%side = "-X";
	else if(%y > 0)
		%side = "+Y";
	else if(%y < 0)
		%side = "-Y";
	else if(%z > 0)
		%side = "+Z";
	else
		%side = "-Z";

	%this.selectedSide = %side;

	//Visualize selected side
	%this.recolor();

	if(%this.selectedSide $= %old)
		return;

	serverPlay3d(BrickRotateSound, %this.getSelectedSideCenter());
}

//Deselect the side
function ND_SelectionBox::deselectSide(%this)
{
	%this.selectedSide = "";

	//Visualize selected side
	%this.recolor();
}

//Move the selected side of the box in or out
function ND_SelectionBox::stepSide(%this, %dir)
{
	%oldP1 = %this.point1;
	%oldP2 = %this.point2;

	if(%dir == -1)
	{
		switch$(%this.selectedSide)
		{
		case "+X":
			%tmp = vectorAdd(%this.point2, "-0.5 0 0");

			if(getWord(vectorSub(%tmp, %this.point1), 0) > 0.4)
				%this.point2 = %tmp;

		case "-X":
			%tmp = vectorAdd(%this.point1, "0.5 0 0");

			if(getWord(vectorSub(%this.point2, %tmp), 0) > 0.4)
				%this.point1 = %tmp;

		case "+Y":
			%tmp = vectorAdd(%this.point2, "0 -0.5 0");

			if(getWord(vectorSub(%tmp, %this.point1), 1) > 0.4)
				%this.point2 = %tmp;

		case "-Y":
			%tmp = vectorAdd(%this.point1, "0 0.5 0");

			if(getWord(vectorSub(%this.point2, %tmp), 1) > 0.4)
				%this.point1 = %tmp;

		case "+Z":
			%tmp = vectorAdd(%this.point2, "0 0 -0.2");

			if(getWord(vectorSub(%tmp, %this.point1), 2) > 0.1)
				%this.point2 = %tmp;

		case "-Z":
			%tmp = vectorAdd(%this.point1, "0 0 0.2");

			if(getWord(vectorSub(%this.point2, %tmp), 2) > 0.1)
				%this.point1 = %tmp;
		}
	}
	else if(%dir == 1)
	{
		switch$(%this.selectedSide)
		{
			case "+X": %this.point2 = vectorAdd(%this.point2, "0.5 0 0");
			case "-X": %this.point1 = vectorAdd(%this.point1, "-0.5 0 0");
			case "+Y": %this.point2 = vectorAdd(%this.point2, "0 0.5 0");
			case "-Y": %this.point1 = vectorAdd(%this.point1, "0 -0.5 0");
			case "+Z": %this.point2 = vectorAdd(%this.point2, "0 0 0.2");
			case "-Z": %this.point1 = vectorAdd(%this.point1, "0 0 -0.2");
		}
	}

	if(%this.point1 $= %oldP1 && %this.point2 $= %oldP2)
	{
		serverPlay3d(errorSound, %this.getSelectedSideCenter());
		return;
	}

	%this.resize(%this.point1, %this.point2);
	serverPlay3d(BrickMoveSound, %this.getSelectedSideCenter());
}

//Get center of currently selected side
function ND_SelectionBox::getSelectedSideCenter(%this)
{
	%halfDiag = vectorSub(%this.point2, %this.point1);

	%halfX = getWord(%halfDiag, 0);
	%halfY = getWord(%halfDiag, 1);
	%halfZ = getWord(%halfDiag, 2);

	switch$(%this.selectedSide)
	{
		case "+X": return vectorSub(%this.point2, 0      SPC %halfY SPC %halfZ);
		case "-X": return vectorAdd(%this.point1, 0      SPC %halfY SPC %halfZ);
		case "+Y": return vectorSub(%this.point2, %halfX SPC 0      SPC %halfZ);
		case "-Y": return vectorAdd(%this.point1, %halfX SPC 0      SPC %halfZ);
		case "+Z": return vectorSub(%this.point2, %halfX SPC %halfY SPC 0     );
		case "-Z": return vectorAdd(%this.point1, %halfX SPC %halfY SPC 0     );
	}
}
