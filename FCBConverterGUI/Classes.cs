namespace FCBConverterGUI
{
    public class HiddenMeshListEntry
    {
		public string Name { set; get; }

		public bool? Enabled { set; get; }
    }

    public class HiddenFacesListEntry
    {
		public string Name { set; get; }
		
		public string FaceStartIndex { set; get; }
		
		public string CountOfFaces { set; get; }
    }
}

