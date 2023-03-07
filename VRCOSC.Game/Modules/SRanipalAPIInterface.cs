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
        if (eyeAvailable) SRanipalAPI.Release(2);
        if (lipAvailable) SRanipalAPI.Release(3);

        EyeStatus.SetDefault();
        LipStatus.SetDefault();
    }

    public void Update()
    {
        if (eyeAvailable) updateEye();
        if (lipAvailable) updateLip();
    }

    private void updateEye()
    {
        SRanipalAPI.GetEyeData(ref EyeData);
    }

    private void updateLip()
    {
        SRanipalAPI.GetLipData(ref LipData);
    }
}
