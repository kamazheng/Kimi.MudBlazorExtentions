export function setTitle(title) {
    document.title = title;
}

//save base64 to as file by js
export function saveAsFile(filename, bytesBase64) {
    let link = document.createElement('a');
    link.download = filename;
    link.href = "data:application/octet-stream;base64," + bytesBase64;
    document.body.appendChild(link); // Needed for Firefox
    link.click();
    document.body.removeChild(link);
}

export function setImgSrc(id, src) {
    document.getElementById(id).attributes['src'] = src;
}

export function getHeight(id) {
    let e = document.getElementById(id);
    if (e != null) {
        return e.offsetHeight;
    }
    else {
        return 0;
    }
}

export function getHeightByClass(className) {
    let e = document.getElementsByClassName(className)[0];
    if (e != null) {
        return e.offsetHeight;
    }
    else {
        return 0;
    }
}

export function setNotScrollMaxHeight(id, desiredMargin) {
    const element = document.getElementById(id);
    if (element) {
        const viewportHeight = window.innerHeight;
        const absoluteTop = getAbsoluteTop(element);
        if (absoluteTop !== null) {
            const height = viewportHeight - absoluteTop - desiredMargin;
            element.style.height = height + 'px';
        }
    }
}

export function setMinHeightToMaxWindowsHeight(id, desiredMargin) {
    const element = document.getElementById(id);
    if (element) {
        const viewportHeight = window.innerHeight;
        const absoluteTop = getAbsoluteTop(element);
        if (absoluteTop !== null) {
            const height = viewportHeight - absoluteTop - desiredMargin;
            element.style.minHeight = height + 'px';
        }
    }
}

export function setNotScrollMaxHeightByClass(className, desiredMargin) {
    const elements = document.getElementsByClassName(className);
    if (elements.length > 0) {
        const viewportHeight = window.innerHeight;
        for (let element of elements) {
            const absoluteTop = getAbsoluteTop(element);
            if (absoluteTop !== null) {
                const height = viewportHeight - absoluteTop - desiredMargin;
                element.style.height = height + 'px';
            }
        }
    }
}

function getAbsoluteTop(element) {
    if (element) {
        const rect = element.getBoundingClientRect ? element.getBoundingClientRect() : null;
        if (rect) {
            const scrollTop = window.scrollY || document.documentElement.scrollTop;
            return rect.top + scrollTop;
        }
    }
    return null;
}

// Prevent Bootstrap dialog from blocking focusin
export function preventBootstrapBlocking() {
    document.addEventListener('focusin', function (e) {
        if (e.target.closest('.tox-tinymce-aux, .moxman-window, .tam-assetmanager-root')) {
            e.stopImmediatePropagation();
        }
    })
}

export function getMaxZIndex() {
    let maxZIndex = 0;
    document.querySelectorAll('*').forEach(function (e) {
        if (e.style.zIndex !== '' && !isNaN(e.style.zIndex)) {
            maxZIndex = Math.max(maxZIndex, parseInt(e.style.zIndex));
        }
    });
    return maxZIndex + 1;
}

// Check if windows dark mode on.
export function isWindowsDarkModeOn() {
    return window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
}


//set all input, textarea, button, select tags inside div to readonly
export function setDivReadOnlyByDivId(divId, isReadOnly) {
    let div = document.getElementById(divId);
    if (div) {
        // Use querySelectorAll to get all the elements in one call
        let elements = div.querySelectorAll('*');
        // Use forEach to iterate over the elements array
        elements.forEach(function (element) {
            // Use a ternary operator to set the attribute based on the element type
            element[element.type === 'button' ? 'disabled' : 'readOnly'] = isReadOnly;
        });
    }
}

export function setDivReadOnlyByDivClassName(divClassName, isReadOnly) {
    let divs = document.getElementsByClassName(divClassName);
    for (let div of divs) {
        let elements = div.querySelectorAll('*');
        elements.forEach(function (element) {
            if (element.tagName === 'FLUENT-SELECT') {
                element.disabled = isReadOnly;
            } else {
                element[element.type === 'button' ? 'disabled' : 'readOnly'] = isReadOnly;
            }
        });
    }
}

export function removeDiv(divId) {
    let div = document.getElementById(divId);
    if (div) {
        div.parentNode.removeChild(div);
    }
}

//get the div children elements raw html by its classname
export function getDivChildrenRawHtmlByClassName(divClassName) {
    let divs = document.getElementsByClassName(divClassName);
    if (divs.length > 0) {
        return divs[0].innerHTML;
    }
}

export function getDivChildrenRawHtml(divId) {
    let div = document.getElementById(divId);
    if (div) {
        return div.innerHTML;
    }
    else {
        return "";
    }
}

export function setAllSubElementsWithSameSelector(divId) {
    // Step 1: Get the div by id and get its selector string (non-id attribute)
    const div = document.getElementById(divId);
    const selector = div.getAttributeNames().find((attr) => attr !== 'id');

    // Check if a non-id attribute is found
    if (selector) {
        const attrValue = div.getAttribute(selector);

        // Step 2: Get the div's sub elements
        const subElements = div.querySelectorAll('*');

        // Step 3: Set the same attribute for all sub elements having the same selector
        subElements.forEach((element) => {
            element.setAttribute(selector, attrValue);
        });
    } else {
        console.log('No non-id attributes found on the specified div.');
    }
}
