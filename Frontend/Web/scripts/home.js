var home = new Vue(
    {
        el: '#home',

        data:
        {
            user: user,
            entries: entries,
            text: ''
        },

        components: {
            'tweet': tweet
        },

        methods:
        {
            deleteEntry: async function (index, id) {

                const response = await fetch(`http://localhost:5000/api/entries/${id}`, {
                    method: 'DELETE',
                    credentials: 'omit',
                    redirect: 'follow',
                    cache: 'no-cache',
                    headers: {
                        'Content-Type': 'application/json',
                        'Authorization': 'Bearer ' + user.token
                    }
                });

                if (response.status == 200) {
                    entries.deleteFromHome(index, id);
                }
            },

            post: async function () {
                const response = await fetch('http://localhost:5000/api/entries', {
                    method: 'POST',
                    credentials: 'omit',
                    redirect: 'follow',
                    cache: 'no-cache',
                    headers: {
                        'Content-Type': 'application/json',
                        'Authorization': 'Bearer ' + user.token
                    },
                    body: JSON.stringify({
                        authorId: user.id,
                        text: this.text
                    })
                });

                if (response.status === 200) {
                    const entryId = parseInt(await response.json());

                    let entry = {
                        id: entryId,
                        author: Object.assign({}, user),
                        authorId: user.id,
                        text: String(this.text)
                    };

                    entries.add(entry);

                    this.text = '';
                    document.getElementById('post').value = '';

                    let toast = new bootstrap.Toast(document.getElementById('successToast'));
                    toast.show();
                }
            },

            getAvatar: function(id) {
                return `http://localhost:5000/api/users/${id}/avatar`;
            }
        },

        computed:
        {
            charactersLeft: function () {
                return this.text.length + '/' + '256';
            }
        }
    });