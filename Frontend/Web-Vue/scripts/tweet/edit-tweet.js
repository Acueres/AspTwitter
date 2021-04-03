var editTweet = new Vue({
    el: '#edit-tweet',

    data:
    {
        loading: false,

        appUser: appUser,
        entry: { text: null },

        textInvalid: false,
    },

    methods:
    {
        post: async function () {
            let textField = document.getElementById('edit-tweet-text');

            let text = textField.value;

            this.textInvalid = text == '';

            if (this.textInvalid) {
                return;
            }

            this.loading = true;
            const response = await fetch(server + `api/entries/${this.entry.id}`, {
                method: 'PATCH',
                credentials: 'omit',
                redirect: 'follow',
                cache: 'no-cache',
                headers: {
                    'Content-Type': 'application/json',
                    'ApiKey': apiKey,
                    'Authorization': 'Bearer ' + appUser.token
                },
                body: JSON.stringify({
                    text: text
                })
            });

            if (response.status === 200) {
                entries.edit(this.entry.id, text);
                appUser.editEntry(this.entry.id, text);

                let modal = bootstrap.Modal.getInstance(document.getElementById('edit-tweet'));
                modal.toggle();
            }

            this.loading = false;
        }
    }
});