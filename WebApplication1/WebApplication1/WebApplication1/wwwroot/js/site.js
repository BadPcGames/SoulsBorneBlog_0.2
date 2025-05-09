export async function getUserRole() {
    try {
        const response = await $.ajax({
            url: '/Auth/GetUserRole', 
            type: 'GET'
        });
        return response;
    } catch (error) {
        console.error("Error getting user status: ", error);
        return null;
    }
}

export async function getUserId() {
    try {
        const response = await $.ajax({
            url: '/Auth/GetUserId',
            type: 'GET'
        });
        return response;
    } catch (error) {
        console.error("Error getting user status: ", error);
        return null;
    }
}

function checkAccess() {
    return new Promise((resolve, reject) => {
        $.ajax({
            url: '/Auth/getAccess',
            type: 'GET',
            success: function (response) {
                if (!response) {
                    alert("Comment caption is not available");
                    resolve(false);
                } else {
                    resolve(true);
                }
            },
            error: function () {
                alert("Comment caption is not available");
                resolve(false);
            }
        });
    });
}

