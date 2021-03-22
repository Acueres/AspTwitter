var userInfoTemplate = {
    props: ['user', 'appUser'],
    methods: {
        getAvatar: function (id) {
            return `http://localhost:5000/api/users/${id}/avatar`;
        },

        openProfile: function (targetUser) {
            if (user.id == targetUser.id) {
                let el = document.querySelector('#profile-tab');
                let tab = new bootstrap.Tab(el);
                tab.show();
                return;
            }
            else {
                explore.openProfile(targetUser);

                let el = document.querySelector('#explore-tab');
                let tab = new bootstrap.Tab(el);
                tab.show();
            }
        }
    },

    template: `
    <div class="row border-bottom p-3">
        <div class="col-auto">
            <img v-bind:src='getAvatar(user.id)' class="img rounded-circle"
                style="width: 50px; height: 50px; outline: none; cursor: pointer"
                v-on:click="openProfile(user)" alt="avatar">
        </div>

        <div class="col-auto" v-on:click="openProfile(user)" style="cursor: pointer">
            <b style="font-size: large;">{{ user.name }}</b>
            <br>
            @{{ user.username }}
        </div>

        <div class="col-auto" v-if="appUser.logged && appUser.id != user.id">
            <button class="btn" v-on:click="appUser.follows(user.id) ? appUser.unfollow(user): appUser.follow(user)"
                v-bind:class="appUser.follows(user.id) ? 'btn-info' : 'btn-outline-info'">
                {{ appUser.follows(user.id) ? 'Unfollow': 'Follow' }}
            </button>
        </div>
    </div>
    `
}