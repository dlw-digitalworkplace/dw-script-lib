var settings = Settings.LoadSettings();
var graphHelper = new GraphHelper(settings);

//await graphHelper.SampleMethods();

var permissions = await graphHelper.GetPermissions(settings!.Item!.SiteId!, settings!.Item!.DocLibName!, settings!.Item!.DriveItemId!);