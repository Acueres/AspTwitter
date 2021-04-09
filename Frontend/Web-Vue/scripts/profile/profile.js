var profile = new Vue({
    el: '#profile',

    data:
    {
        appUser: appUser,
        entries: entries,

        contentType: 'Tweets',
        followerType: 'Followers'
    },

    components: {
        'tweet': tweetTemplate,
        'profile': profileTemplate,
        'user-info': userInfoTemplate
    },

    methods:
    {
        deleteEntry: async function (id) {

            const response = await fetch(server + `api/entries/${id}`, {
                method: 'DELETE',
                credentials: 'omit',
                redirect: 'follow',
                cache: 'no-cache',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': 'Bearer ' + appUser.token
                }
            });

            if (response.status == 200) {
                entries.delete(id);
                appUser.deleteEntry(id);
            }
        },

        getContent: function () {
            if (this.contentType == 'Tweets') {
                return appUser.entries;
            }

            if (this.contentType == 'Likes') {
                return appUser.favorites;
            }

            return appUser.retweets;
        },

        getFollowers: function () {
            if (this.followerType == 'Followers') {
                return appUser.followers;
            }

            return appUser.following;
        }
    }
});