namespace CSLox
{
	internal class ReturnException : Exception
	{
		internal readonly object value;
		
		internal ReturnException(object value) : base(null, null)
		{
			this.value = value;
		}
	}
}