var home = new Vue(
    {
        el: '#home',

        data:
        {
            user: user,
            entries: [],
            text: ''
        },

        created: async function () {
            const response = await fetch(`http://localhost:5000/api/entries`);
            const data = await response.json();
            this.entries = data;
            this.entries.reverse();
        },

        methods:
        {
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
                    this.entries.splice(0, 0, {
                        author: user,
                        authorId: user.id,
                        text: String(this.text)
                    });

                    this.text = '';
                    document.getElementById('post').value = '';

                    let toast = new bootstrap.Toast(document.getElementById('successToast'));
                    toast.show();
                }
            }
        },

        computed:
        {
            charactersLeft: function () {
                if (this.text.length == 0) {
                    return null;
                }

                return this.text.length + '/' + '256';
            }
        }
    });