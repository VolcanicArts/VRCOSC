// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRChat.API.Api;

namespace VRCOSC.App.VRChatAPI;

public class VRChatAPIClient
{
    internal AuthenticationHandler AuthHandler { get; } = new();

    public ISystemApi? System { get; private set; }
    public IUsersApi? Users { get; private set; }
    public IAvatarsApi? Avatars { get; private set; }
    public IWorldsApi? Worlds { get; private set; }
    public IFriendsApi? Friends { get; private set; }
    public IEconomyApi? Economy { get; private set; }
    public IFavoritesApi? Favorites { get; private set; }
    public IGroupsApi? Groups { get; private set; }
    public IFilesApi? Files { get; private set; }
    public IInstancesApi? Instances { get; private set; }
    public IPermissionsApi? Permissions { get; private set; }
    public IInviteApi? Invite { get; private set; }
    public IPlayermoderationApi? PlayerModeration { get; private set; }
    public INotificationsApi? Notifications { get; private set; }

    public VRChatAPIClient()
    {
        AuthHandler.State.Subscribe(newState =>
        {
            if (newState == AuthenticationState.LoggedIn)
            {
                System = new SystemApi(AuthHandler.Configuration);
                Users = new UsersApi(AuthHandler.Configuration);
                Avatars = new AvatarsApi(AuthHandler.Configuration);
                Worlds = new WorldsApi(AuthHandler.Configuration);
                Friends = new FriendsApi(AuthHandler.Configuration);
                Economy = new EconomyApi(AuthHandler.Configuration);
                Favorites = new FavoritesApi(AuthHandler.Configuration);
                Groups = new GroupsApi(AuthHandler.Configuration);
                Files = new FilesApi(AuthHandler.Configuration);
                Instances = new InstancesApi(AuthHandler.Configuration);
                Permissions = new PermissionsApi(AuthHandler.Configuration);
                Instances = new InstancesApi(AuthHandler.Configuration);
                Invite = new InviteApi(AuthHandler.Configuration);
                PlayerModeration = new PlayermoderationApi(AuthHandler.Configuration);
                Notifications = new NotificationsApi(AuthHandler.Configuration);
            }

            if (newState == AuthenticationState.LoggedOut)
            {
                System = null;
                Users = null;
                Avatars = null;
                Worlds = null;
                Friends = null;
                Economy = null;
                Favorites = null;
                Groups = null;
                Files = null;
                Instances = null;
                Permissions = null;
                Instances = null;
                Invite = null;
                PlayerModeration = null;
                Notifications = null;
            }
        });
    }
}
