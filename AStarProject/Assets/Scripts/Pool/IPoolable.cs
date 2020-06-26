namespace Pool
{
	public interface IPoolable
	{
		string PrefabId { get; set; }
		void SaveDefaultValues();
		void SetDefaultValues();
		void OnPop();
		void OnPush();
	}

}
