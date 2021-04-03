var explore = new Vue({
    el: '#explore',

    data:
    {
        appUser: appUser,

        users: [],
        entries: [],
        selectedUser: new User(),

        contentType: 'Tweets',
        followerType: 'Followers',

        showProfile: false,
        searchUsers: true
    },

    components: {
        'tweet': tweetTemplate,
        'profile': profileTemplate,
        'user-info': userInfoTemplate
    },

    methods: {
        sendQuery: async function () {
            let entity = 'entries';
            if (this.searchUsers) {
                entity = 'users';
            }

            this.users = [];
            this.entries = [];

            let query = document.getElementById('search-bar').value;
            if (query == '') {
                return;
            }

            const response = await fetch(server + `api/${entity}/search`, {
                method: 'POST',
                cache: 'no-cache',
                credentials: 'omit',
                redirect: 'follow',
                headers: {
                    'Content-Type': 'application/json',
                    'ApiKey': apiKey
                },
                body: JSON.stringify(query)
            })

            if (response.status != 400) {
                this.showProfile = false;

                if (this.searchUsers) {
                    this.users = await response.json();
                }
                else {
                    this.entries = await response.json();
                    this.entries.reverse();
                }
            }
        },

        loadUser: async function (id) {
            this.selectedUser = new User(id);
            this.showProfile = true;

            await this.selectedUser.load();
        },

        getContent: function () {
            if (this.contentType == 'Tweets') {
                return this.selectedUser.entries;
            }

            if (this.contentType == 'Likes') {
                return this.selectedUser.favorites;
            }

            return this.selectedUser.retweets;
        },

        getFollowers: function () {
            if (this.followerType == 'Followers') {
                return this.selectedUser.followers;
            }

            return this.selectedUser.following;
        }
    }
});