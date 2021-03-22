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
      if (!user.logged) {
        return false;
      }

      return user.favorites.includes(id);
    },

    retweeted: function (id) {
      if (!user.logged) {
        return false;
      }

      return user.retweets.some(x => x.id == id);
    },

    addLike: function (entry) {
      if (!user.logged) {
        return;
      }

      user.favorites.push(entry.id);
      entry.likeCount++;
      fetch(`http://localhost:5000/api/entries/${entry.id}/favorite`, {
        method: 'POST',
        credentials: 'omit',
        redirect: 'follow',
        cache: 'no-cache',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer ' + user.token
        }
      });
    },

    removeLike: function (entry) {
      if (!user.logged) {
        return;
      }

      let index = user.favorites.indexOf(entry.id);
      user.favorites.splice(index, 1);
      entry.likeCount--;
      fetch(`http://localhost:5000/api/entries/${entry.id}/favorite`, {
        method: 'DELETE',
        credentials: 'omit',
        redirect: 'follow',
        cache: 'no-cache',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer ' + user.token
        }
      });
    },

    retweet: function (entry) {
      if (!user.logged || user.id == entry.authorId) {
        return;
      }

      user.retweets.push(entry);
      entry.retweetCount++;
      user.addEntry(entry);

      fetch(`http://localhost:5000/api/entries/${entry.id}/retweet`, {
        method: 'POST',
        credentials: 'omit',
        redirect: 'follow',
        cache: 'no-cache',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer ' + user.token
        }
      });
    },

    removeRetweet: function (entry) {
      if (!user.logged) {
        return;
      }

      let index = user.retweets.findIndex(x => x.id == entry.id);
      user.retweets.splice(index, 1);
      entry.retweetCount--;
      user.deleteEntry(entry.id);

      fetch(`http://localhost:5000/api/entries/${entry.id}/retweet`, {
        method: 'DELETE',
        credentials: 'omit',
        redirect: 'follow',
        cache: 'no-cache',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Bearer ' + user.token
        }
      });
    },

    openTweet: function (entry) {
      tweet.load(entry);
    },

    openProfile: function (targetUser) {
      if (user.id == targetUser.id) {
        let el = document.querySelector('#profile-tab');
        let tab = new bootstrap.Tab(el);
        tab.show();
        return;
      }
      else {
        explore.openProfile(targetUser);

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
          'Authorization': 'Bearer ' + user.token
        }
      });

      if (response.status == 200) {
        entries.delete(id);
        user.deleteEntry(id);
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
        <b style="font-size: large; cursor: pointer" v-on:click="openProfile(entry.author)">
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