var tweetTemplate = {
  props: ['entry', 'user', 'showRetweets'],
  methods:
  {
    displayCount: function (n) {
      if (n >= 1e6) {
        let res = Math.round(n / 1e5) / 10;

        return res.toString() + 'M';
      }

      else if (n >= 1e3) {
        let res = Math.round(n / 1e2) / 10;

        return res.toString() + 'K';
      }

      return n > 0 ? n : null;
    },

    liked: function (id) {
      if (!appUser.logged) {
        return false;
      }

      return appUser.favorites.some(x => x.id == id);
    },

    retweeted: function (id) {
      if (!appUser.logged) {
        return false;
      }

      return appUser.retweets.some(x => x.id == id);
    },

    addLike: function (entry) {
      if (!appUser.logged) {
        return;
      }

      appUser.favorites.push(entry);
      entry.likeCount++;
      fetch(`http://localhost:5000/api/entries/${entry.id}/favorite`, {
        method: 'POST',
        credentials: 'omit',
        redirect: 'follow',
        cache: 'no-cache',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer ' + appUser.token
        }
      });
    },

    removeLike: function (entry) {
      if (!appUser.logged) {
        return;
      }

      let index = appUser.favorites.findIndex(x => x.id == entry.id);
      appUser.favorites.splice(index, 1);
      entry.likeCount--;
      fetch(`http://localhost:5000/api/entries/${entry.id}/favorite`, {
        method: 'DELETE',
        credentials: 'omit',
        redirect: 'follow',
        cache: 'no-cache',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer ' + appUser.token
        }
      });
    },

    retweet: function (entry) {
      if (!appUser.logged || appUser.id == entry.authorId) {
        return;
      }

      appUser.retweets.push(entry);
      entry.retweetCount++;
      appUser.addEntry(entry);

      fetch(`http://localhost:5000/api/entries/${entry.id}/retweet`, {
        method: 'POST',
        credentials: 'omit',
        redirect: 'follow',
        cache: 'no-cache',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer ' + appUser.token
        }
      });
    },

    removeRetweet: function (entry) {
      if (!appUser.logged) {
        return;
      }

      let index = appUser.retweets.findIndex(x => x.id == entry.id);
      appUser.retweets.splice(index, 1);
      entry.retweetCount--;
      appUser.deleteEntry(entry.id);

      fetch(`http://localhost:5000/api/entries/${entry.id}/retweet`, {
        method: 'DELETE',
        credentials: 'omit',
        redirect: 'follow',
        cache: 'no-cache',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer ' + appUser.token
        }
      });
    },

    openTweet: function (entry) {
      tweet.load(entry);
    },

    openProfile: function (targetUser) {
      if (appUser.id == targetUser.id) {
        let el = document.querySelector('#profile-tab');
        let tab = new bootstrap.Tab(el);
        tab.show();
        return;
      }
      else {
        explore.loadUser(targetUser.id);

        let el = document.querySelector('#explore-tab');
        let tab = new bootstrap.Tab(el);
        tab.show();
      }
    },

    getAvatar: function (id) {
      return `http://localhost:5000/api/users/${id}/avatar`;
    },

    getDate: function (timestamp) {
      let date = new Date(timestamp).toDateString();
      date = date.split(' ');

      return `${date[1]} ${date[2]}`;
    },

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
    }
  },
  template: `
    <div class="row border-bottom p-3">
      <div class="pb-3" v-if="showRetweets && entry.authorId != user.id">
        <i class="la la-retweet" aria-hidden="true"></i>
        {{ user.name }} Retweeted <br>
      </div>

      <div class="col-auto">
        <img v-bind:src='getAvatar(entry.authorId)' class="img rounded-circle"
          style="width: 50px; height: 50px; outline: none; cursor: pointer"
          v-on:click="openProfile(entry.author)" alt="avatar">
      </div>

      <div class="col-auto">
        <b class="text-truncate" style="font-size: large; cursor: pointer" v-on:click="openProfile(entry.author)">
          {{ entry.author.name }}
        </b>
        @{{ entry.author.username }}
        - {{ getDate(entry.timestamp) }}

        <button type="button" class="btn btn-default" aria-label="Left Align"
          v-on:click='deleteEntry(entry.id)' v-if="user.logged && entry.authorId == user.id">
          <span class="la la-trash" aria-hidden="true"></span>
        </button> <br>
        <a style="color: black; word-wrap: break-word;">{{ entry.text }}</a> <br>

        <div class="btn-group" role="group" aria-label="Basic example">
          <button type="button" class="btn btn-default shadow-none" aria-label="Left Align"
           v-on:click="openTweet(entry)" data-bs-toggle="modal" data-bs-target="#tweet">
            <span class="la la-comment-dots" aria-hidden="true"></span>
            <span>{{ displayCount(entry.commentCount) }}</span>
          </button>

          <button type="button" class="btn btn-default shadow-none" aria-label="Left Align">
            <span class="la la-retweet" aria-hidden="true" v-bind:style="retweeted(entry.id) ? 'color: blue;': ''"
            v-on:click="retweeted(entry.id) ? removeRetweet(entry): retweet(entry)"></span>
            <span>{{ displayCount(entry.retweetCount) }}</span>
          </button>

          <button type="button" class="btn btn-default shadow-none" aria-label="Left Align">
            <span class="la la-heart" aria-hidden="true" v-bind:style="liked(entry.id) ? 'color: blue;': ''"
             v-on:click="liked(entry.id) ? removeLike(entry): addLike(entry)"></span>
            <span>{{ displayCount(entry.likeCount) }}</span>
          </button>
        </div>

      </div>
    </div>`
}