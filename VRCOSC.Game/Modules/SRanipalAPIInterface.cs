using System.Threading.Tasks;
using osu.Framework.Bindables;
using SRanipalLib;

namespace VRCOSC.Game.Modules;

public class SRanipalAPIInterface
{
    public readonly Bindable<Error> EyeStatus = new(Error.UNDEFINED);
    public readonly Bindable<Error> LipStatus = new(Error.UNDEFINED);

    public LipDataV2 LipData;
    public EyeDataV2 EyeData;

    private bool eyeAvailable => EyeStatus.Value == Error.WORK;
    private bool lipAvailable => LipStatus.Value == Error.WORK;

    public void Initialise(bool eye, bool lip)
    {
        Task.Run(() =>
        {
            if (eye) EyeStatus.Value = SRanipalAPI.Initialise(2);
            if (lip) LipStatus.Value = SRanipalAPI.Initialise(3);
        });
    }

    public void Release()
    {
        SRanipalAPI.Release(2);
        SRanipalAPI.Release(3);

        EyeStatus.SetDefault();
        LipStatus.SetDefault();
    }

    public void Update()
    {
        if (eyeAvailable) SRanipalAPI.GetEyeData(ref EyeData);
        if (lipAvailable) SRanipalAPI.GetLipData(ref LipData);
    }
}
