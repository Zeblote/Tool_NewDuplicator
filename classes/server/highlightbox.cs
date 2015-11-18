// * ######################################################################
// *
// *    New Duplicator - Classes - Server
// *    ND_HighlightBox
// *
// *    -------------------------------------------------------------------
// *    Resizable and recolorable box frame using 12 static shapes
// *
// * ######################################################################

//Create a new highlight box
function ND_HighlightBox()
{
	ND_ServerGroup.add(
		%this = new ScriptObject(ND_HighlightBox)
	);

	for(%i = 0; %i < 4; %i++)
	{
		%this.border_x[%i] = new StaticShape(){datablock = ND_SelectionCubeBorder;};
		%this.border_y[%i] = new StaticShape(){datablock = ND_SelectionCubeBorder;};
		%this.border_z[%i] = new StaticShape(){datablock = ND_SelectionCubeBorder;};
	}

	%this.color = "1 0.84 0 1";
	%this.applyColors();

	return %this;
}

//Destroy static shapes when highlight box is removed
function ND_HighlightBox::onRemove(%this)
{
	for(%i = 0; %i < 4; %i++)
	{
		%this.border_x[%i].delete();
		%this.border_y[%i].delete();
		%this.border_z[%i].delete();
	}
}

//Apply color changes to the highlight box
function ND_HighlightBox::applyColors(%this)
{
	for(%i = 0; %i < 4; %i++)
	{
		%this.border_x[%i].setNodeColor("ALL", %this.color);
		%this.border_y[%i].setNodeColor("ALL", %this.color);
		%this.border_z[%i].setNodeColor("ALL", %this.color);
	}
}

//Return current size of highlight box
function ND_HighlightBox::getSize(%this)
{
	return %this.point1 SPC %this.point2;
}

//Resize the highlight box
function ND_HighlightBox::resize(%this, %point1, %point2)
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

	%maxLen = getMax(getMax(%len_x, %len_y), %len_z);

	if(%maxLen > 1024)
		%width = 7;
	else if(%maxLen > 512)
		%width = 6;
	else if(%maxLen > 256)
		%width = 5;
	else if(%maxLen > 128)
		%width = 4;
	else if(%maxLen > 64)
		%width = 3;
	else if(%maxLen > 32)
		%width = 2;
	else if(%maxLen > 4)
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
