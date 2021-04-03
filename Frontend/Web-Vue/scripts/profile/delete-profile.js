var deleteProfile = new Vue({
    el: '#delete-profile',

    methods:
    {
        close: function () {
            let modal = bootstrap.Modal.getInstance(document.getElementById('delete-profile'));
            modal.toggle();
        },

        deleteProfile: async function () {
            const response = await fetch(server + `api/users/${appUser.id}`, {
                method: 'DELETE',
                cache: 'no-cache',
                credentials: 'omit',
                redirect: 'follow',
                headers: {
                    'Content-Type': 'application/json',
                    'ApiKey': apiKey,
                    'Authorization': 'Bearer ' + appUser.token
                }
            });

            if (response.status == 200) {
                let modal = bootstrap.Modal.getInstance(document.getElementById('delete-profile'));
                modal.toggle();

                let el = document.querySelector('#home-tab');
                let tab = new bootstrap.Tab(el);

                appUser.clear();

                tab.show();
            }
        }
    }
});