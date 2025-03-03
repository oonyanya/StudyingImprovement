function Inject_fn() {
    const MARU = 'O';
    const BATSU = 'X';
    const SANKAKU = '？';
    const FOLDING_LENGTH_IN_BRAKET = 30;

    function add_temp_marker(elements) {
        add_temp_marker(elements, null);
    }
    function add_temp_marker(elements, matching) {
        elements.forEach((e) => {
            if (e.textContent.match(matching) == null) {
                return;
            }

            const new_item1 = document.createElement('span');
            new_item1.className = "temp_checkbox mark";
            new_item1.innerHTML = MARU;

            const new_item2 = document.createElement('span');
            new_item2.className = "temp_checkbox mark";
            new_item2.innerHTML = BATSU;

            const new_item4 = document.createElement('span');
            new_item4.className = "temp_checkbox mark";
            new_item4.innerHTML = SANKAKU;

            e.append(new_item1, new_item2, new_item4);
        });
    }

    function add_temp_marker_for_text(elements, matching) {
        elements.forEach((e) => {
            replace_element(e, (s) => {
                let new_s = s.
                    replaceAll(matching,
                        "<span class='temp_checkbox strikeline'>$1</span>"
                    );
                return new_s;
            });
        });
    }

    function isHTML(htmlString) {
        let parser = new DOMParser();
        let doc = parser.parseFromString(htmlString, "application/xml");
        let errorNode = doc.querySelector('parsererror');
        return !errorNode;
    }

    function replace_element(element, f) {
        element.innerHTML = f(element.innerHTML);
    }

    function wash_string_for_sreen_reader(s) {
        /* EDGEのスクリーンリーダーが途中で止まってしまう対策 */
        return s.replaceAll("・", "、");
    }

    function add_folding_marker(elements) {
        const id = 0;
        elements.forEach((e) => {
            replace_element(e, (s) => {
                let new_s = s.
                    replaceAll(/（/g, '<span class=\'folding_box\'>※<span>（').
                    replaceAll(/）/g, '）</span></span>');
                return wash_string_for_sreen_reader(new_s);
            });
            let child_elements = e.querySelectorAll('.folding_box');
            child_elements.forEach((ce) => {
                console.log(ce.textContent);
                if (ce.textContent.length < FOLDING_LENGTH_IN_BRAKET) {
                    ce.classList.add('touch');
                }
            });
        });
    }

    function wash_element_for_screen_reader(elements) {
        elements.forEach((e) => {
            replace_element(e, (s) => {
                return wash_string_for_sreen_reader(s);
            });
        });
    }

    function add_temp_memo(element) {
        if (element == null) return;
        let memo_element = document.createElement("textarea");
        memo_element.className = "temp_memo";
        memo_element.setAttribute("placeholder", "It is for temporary memo");
        element.append(memo_element);
    }

    let elements = document.querySelectorAll('.checkable_list_item');
    add_temp_marker(elements);

    elements = document.querySelectorAll('div.question_text > div > p');
    add_temp_marker_for_text(elements, /([1-9１-９]\s*[ァ-ヴ]+)/g); /*「１ アウ ２ イウ」に一致 */

    add_temp_memo(document.querySelector("div.question_item"));

    elements = document.querySelectorAll('#doc > h1');
    wash_element_for_screen_reader(elements);

    elements = document.querySelectorAll('#doc > h2');
    wash_element_for_screen_reader(elements);

    elements = document.querySelectorAll('#doc > h3');
    wash_element_for_screen_reader(elements);

    elements = document.querySelectorAll('#doc > p');
    add_folding_marker(elements);

    elements = document.querySelectorAll('#doc > table > tbody > tr > td');
    add_folding_marker(elements);

    elements = document.querySelectorAll('#doc > ul > li');
    add_folding_marker(elements);

    elements = document.querySelectorAll('#doc > ol > li');
    add_folding_marker(elements);

    elements = document.querySelectorAll('#answer_box_on > div > div > div > div > p');
    add_folding_marker(elements);

    /* video.jsを使っているようなので、モバイルデーターの節約のため自動再生を止める */
    if (typeof (videojs) != 'undefined') {
        console.log("video js hook start");
        videojs.hook('beforesetup', function (videoEl, options) {
            console.log("video js hook success");
            options.autoplay = false;
            options.preload = 'none';
            return options;
        });
    }
}

nodeIterator = null;
/* ひとつ前の要素 */
preElement = null;
/* 中断されたかどうか */
isPaused = false;
/* 次方向に再生するかどうか。一時的に再生方向を変えても自動的に次に移動する */
isSpeakDirectionNext = true;
/* 再生ボタンを追加する */
function onStartTextToSpeech() {
    var selection = document.getSelection();
    if (selection.anchorNode != null) {
        StartSpeakText(selection.anchorNode);
    }
}
/* 音声読み上げを開始する */
function StartSpeakText(start_element) {
    var hasnode = false;
    nodeIterator = document.createNodeIterator(document.body, 4);
    if (start_element != null) {
        while (nodeIterator.nextNode() != null) {
            if (nodeIterator.referenceNode.parentElement === start_element.parentElement) {
                hasnode = true;
                break;
            }
        }
        if (hasnode == false)
            nodeIterator = document.createNodeIterator(document.body, 4);
    }
    if (hasnode == false)
        nodeIterator.nextNode();
    preElement = null;
    currentElement = null;
    SpeakText();
}
/* 音声読み上げを実行する */
function requestSpeak(text) {
    if (window.HybridWebView == "undefined")
        return;
    window.HybridWebView.SendMessageToDotNet(1, JSON.stringify({ "MethodName": "speakText", "ParamValues": [text] }));
}
/* 表示判定を行う */
function isFullyVisible(elem, tolerance = 0.5) {
    const rect = elem.getBoundingClientRect();
    const windowHeight = (window.innerHeight || document.documentElement.clientHeight);
    const windowWidth = (window.innerWidth || document.documentElement.clientWidth);

    /* Check with tolerance for being within the viewport */
    const inViewVertically = rect.top <= windowHeight + tolerance && rect.bottom >= -tolerance;
    const inViewHorizontally = rect.left <= windowWidth + tolerance && rect.right >= -tolerance;
    const inViewport = inViewVertically && inViewHorizontally;

    /* Check for CSS visibility, 'hidden' attribute, and dimensions */
    const style = getComputedStyle(elem);
    const notHiddenByCSS = style.display !== 'none' && style.visibility !== 'hidden' && parseFloat(style.opacity) > 0;
    const notHiddenAttribute = !elem.hidden;
    const hasDimensions = elem.offsetWidth > 0 || elem.offsetHeight > 0 || elem.getClientRects().length > 0;

    return inViewport && notHiddenByCSS && notHiddenAttribute && hasDimensions;
}
/* 現在のノードのテキストを読み上げ、次のノードに移動する */
function SpeakText() {
    var currentElement = nodeIterator.referenceNode;
    if (currentElement != null) {
        if (preElement != null && preElement != currentElement)
            preElement.parentElement.classList.remove("text_reading");
        currentElement.parentElement.classList.add("text_reading");
        if (currentElement.parentElement.scrollIntoViewIfNeeded != typeof (undefined)) {
            currentElement.parentElement.scrollIntoViewIfNeeded(false);
        } else {
            currentElement.parentElement.scrollIntoView(false);
        }
        if (isFullyVisible(currentElement.parentElement)) {
            requestSpeak(currentElement.textContent);
        } else {
            requestSpeak("");   /* 空文字を送るとスキップして次のノードに行くのと同じ効果が生まれる */
        }
        preElement = currentElement;
    } else {
        preElement = null;
        nodeIterator = null;
    }
}
/* 音声読み上げが完了した */
function onSpeakReadFinish() {
    if (window.HybridWebView == "undefined")
        return;
    if (isPaused == false) {
        if (isSpeakDirectionNext) {
            nodeIterator.nextNode();
        } else {
            nodeIterator.previousNode();
            isSpeakDirectionNext = true;
        }
        SpeakText();
    }
}
/* 音声読み上げが中断された */
function onSpeakReadPause() {
    preElement.parentElement.classList.remove("text_reading");
    isPaused = true;
}
/* 音声読み上げがキャンセルされた */
function onSpeakReadCancel() {
    if (preElement != null) {
        preElement.parentElement.classList.remove("text_reading");
    }
    preElement = null;
    nodeIterator = null;
    isPaused = true;
}
/* 音声読み上げボタンが押された */
function onSpeakPlayStart() {
    isPaused = false;
    if (nodeIterator != null) {
        SpeakText();
    } else {
        StartSpeakText();
    }
}
/* 前の単語や段落に移動するボタンが押された */
function onSpeakPrev() {
    /* すでにキャンセルされてるので、一時的に前方向にする */
    isSpeakDirectionNext = false;
}
/* 次の単語や段落に移動するボタンが押された */
function onSpeakNext() {
    /* すでにキャンセルされてるので、次方向にする */
    isSpeakDirectionNext = true;
}

function Inject_fn_onload() {
    /* iframがある場合、どうせ後で読み込まれる */
    if (typeof (jQuery) == "undefined")
        return;
    /* 暗記マーカー機能が破壊されたので再度定義する(ソース：member.studying.jp/skin/common/js/doc.js) */
    $('.span-memory').unbind('click').click(function () {
        if ($(this).hasClass("touch")) {
            $(this).removeClass("touch");
        } else {
            $(this).addClass("touch");
        }
    });
    /* 折り畳み */
    $('.folding_box').unbind('click').click(function () {
        if ($(this).hasClass("touch")) {
            $(this).removeClass("touch");
        } else {
            $(this).addClass("touch");
        }
        /* 暗記ノートの部分が親要素の場合、親に影響が及ぶと変な動作をしてしまう */
        return false;
    });
    /* 一時チェック */
    $('.temp_checkbox').unbind('click').click(function () {
        if ($(this).hasClass("touch")) {
            $(this).removeClass("touch");
        } else {
            $(this).addClass("touch");
        }
        /* 暗記ノートの部分が親要素の場合、親に影響が及ぶと変な動作をしてしまう */
        return false;
    });
}

function Inject_fn_oninteractive() {
    Inject_fn();
    window.addEventListener("load", () => {
        Inject_fn_onload();
    });
}

console.log("document state:" + document.readyState);
if (document.readyState == 'loading') {
    document.addEventListener("DOMContentLoaded", () => {
        Inject_fn_oninteractive();
    });
} else if (document.readyState == 'interactive') {
    Inject_fn_oninteractive();
} else {
    Inject_fn();
    Inject_fn_onload();
}