namespace FCBConverterGUI
{
	public class HiddenMeshListEntry
	{
		public string ID { set; get; }

		public string Name { set; get; }

		public bool? Enabled { set; get; }
	}

	public class HiddenFacesListEntry
	{
		public string ID { set; get; }

		public string Name { set; get; }
	
		public int FaceStartIndex { set; get; }
	
		public int CountOfFaces { set; get; }
	}
}
