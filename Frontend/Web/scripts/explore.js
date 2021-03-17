var explore = new Vue({
    el: '#explore',

    data:
    {
        user: user,

        users: [],
        entries: [],
        selectedUser: null,
        selectedUserEntries: [],

        showProfile: false,
        searchUsers: true
    },

    components: {
        'tweet': tweetTemplate,
        'profile': profileTemplate
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
            const response = await fetch(`http://localhost:5000/api/${entity}/search`, {
                method: 'POST',
                cache: 'no-cache',
                credentials: 'omit',
                redirect: 'follow',
                headers: { 'Content-Type': 'application/json' },
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

        openProfile: async function (user) {
            this.selectedUser = user;
            this.showProfile = true;

            let response = await fetch(`http://localhost:5000/api/users/${this.selectedUser.id}/entries`);
            this.selectedUserEntries = await response.json();
            this.selectedUserEntries.reverse();
        },

        //Empty function for compatibility reasons
        deleteEntry: function (id) {  },

        getAvatar: function (id) {
            return `http://localhost:5000/api/users/${id}/avatar`;
        }
    }
});