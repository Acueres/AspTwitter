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
            'tweet': tweetTemplate,
            'user-info': userInfoTemplate
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
                    const entryId = parseInt(await response.json());

                    let entry = {
                        id: entryId,
                        author: Object.assign({}, user),
                        authorId: user.id,
                        text: String(this.text),
                        timestamp: new Date().toUTCString()
                    };

                    entries.add(entry);
                    user.addEntry(entry);

                    this.text = '';
                    document.getElementById('post').value = '';
                }
            }
        }
    });