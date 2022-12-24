namespace WingetGUI.Core.Models;
public class UpgradablePackage
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string CurrentVersion { get; set; }
    public string NextVersion { get; set; }
    public string Publisher { get; set; }
}
