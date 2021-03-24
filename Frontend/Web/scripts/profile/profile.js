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

            const response = await fetch(`http://localhost:5000/api/entries/${id}`, {
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

        uploadImage: function () {
            let imageInput = document.getElementById("imageInput");
            let avatar = document.getElementById("editAvatar");

            let reader = new FileReader();

            let mb = 1024 * 1024;

            if (imageInput.files && imageInput.files[0] && imageInput.files[0].size <= mb) {
                reader.onload = (e) => {
                    avatar.src = e.target.result;
                }
                reader.readAsDataURL(imageInput.files[0]);

                let image = document.getElementById("imageInput").files[0];
                let form = new FormData();
                form.append("avatar", image);

                let settings = {
                    "async": true,
                    "crossDomain": true,
                    "url": `http://localhost:5000/api/users/${appUser.id}/avatar`,
                    "method": "POST",
                    "processData": false,
                    "contentType": false,
                    "mimeType": "multipart/form-data",
                    "data": form,
                    beforeSend: function (xhr) {
                        xhr.setRequestHeader("Authorization", 'Bearer ' + appUser.token);
                    }
                };

                jQuery.ajax(settings);
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