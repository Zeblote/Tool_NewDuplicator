// Handles the undo stack for duplicator actions.
// -------------------------------------------------------------------

package NewDuplicator_Server
{
	//Catch things falling off the end of the undo stack
	function QueueSO::push(%obj, %val)
	{
		%lastVal = %obj.val[(%obj.head + 1) % %obj.size];

		if(getFieldCount(%lastVal) == 2)
		{
			%str = getField(%lastVal, 1);

			if(%str $= "ND_PLANT"
			|| %str $= "ND_PAINT"
			|| %str $= "ND_WRENCH")
				getField(%lastVal, 0).delete();
		}

		parent::push(%obj, %val);
	}
};
