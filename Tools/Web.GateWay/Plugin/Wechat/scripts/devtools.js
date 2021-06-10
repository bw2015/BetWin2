chrome.devtools.network.getHAR(function (result) {
    chrome.extension.sendRequest(result);
    var entries = result.entries;
    if (!entries.length) {
        console.warn("ChromeFirePHP suggests that you reload the page to track" +
            " FirePHP messages for all the requests");
    }

    chrome.devtools.network.onRequestFinished.addListener(function (xhr) {
        chrome.extension.sendRequest(xhr);
    });
});