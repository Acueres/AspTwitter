var profileTemplate = {
  props: ['user', 'appUser'],
  methods:
  {
    displayCount: function (n) {
      if (n >= 1e6) {
        let res = Math.round(n / 1e5) / 10;

        return res.toString() + 'M';
      }

      else if (n >= 1e3) {
        let res = Math.round(n / 1e2) / 10;

        return res.toString() + 'K';
      }

      return n;
    },

    getAvatar: function (id) {
      return server + `api/users/${id}/avatar`;
    },

    openAvatarSelector: function () {
      document.getElementById('avatar-selector').click();
    },
  },

  template: `
    <div class="position-relative p-3 border-bottom bg-light">
        <input type="image" id="edit-avatar" v-bind:src='getAvatar(user.id)' v-on:click="appUser.logged && appUser.id != user.id ? null: openAvatarSelector()"
                class="img rounded-circle float-left" style="width: 100px; height: 100px; outline: none;" alt="avatar">

        <button v-if="appUser.id == user.id" class="btn btn-success position-absolute m-3 top-20 end-0" data-bs-toggle="modal"
                data-bs-target="#edit-profile" type="button">Edit profile</button>
        <button v-if="appUser.id == user.id" type="button" class="btn btn-secondary position-absolute m-3 bottom-0 end-0" data-bs-toggle="modal"
                    data-bs-target="#delete-profile">Delete profile</button>
        <button v-else class="btn position-absolute m-3 top-20 end-0"
                v-on:click="appUser.follows(user.id) ? appUser.unfollow(user): appUser.follow(user)"
                v-bind:class="appUser.follows(user.id) ? 'btn-info' : 'btn-outline-info'"
                type="button">{{ appUser.follows(user.id) ? 'Unfollow': 'Follow' }}</button>

        <p class="text-truncate">
            <b style="font-size: large;">{{ user.name }}</b>
            <br>
            @{{ user.username }}
        </p>
        <p style="color: black; word-wrap: break-word;">{{ user.about }}</p>
        <b style="font-size: large;"> {{ displayCount(user.followingCount) }} </b> Following 
        <b style="font-size: large;"> {{ displayCount(user.followerCount) }} </b> Followers
      </div>`
}