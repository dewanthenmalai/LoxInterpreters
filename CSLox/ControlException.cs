namespace CSLox.ControlException
{
	internal class BreakException : Exception
	{
		internal BreakException() : base(null, null)
		{
			
		}
	}
	
	internal class ContinueException : Exception
	{
		internal ContinueException() : base(null, null)
		{
			
		}
	}
}