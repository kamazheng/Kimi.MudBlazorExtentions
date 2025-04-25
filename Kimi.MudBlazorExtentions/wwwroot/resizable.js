export function startDrag(dotNetObj, direction, initialPosition, initialSize) {
    const onMouseMove = (e) => {
        const currentPosition = direction === "horizontal" ? e.clientX : e.clientY;
        const delta = currentPosition - initialPosition;
        const newSize = initialSize + delta;
        dotNetObj.invokeMethodAsync('UpdateSize', direction, newSize);
    };

    const onMouseUp = () => {
        document.removeEventListener('mousemove', onMouseMove);
        document.removeEventListener('mouseup', onMouseUp);
    };

    document.addEventListener('mousemove', onMouseMove);
    document.addEventListener('mouseup', onMouseUp);
}

export function getParentSize(element, property) {
    return element.parentElement[property];
}