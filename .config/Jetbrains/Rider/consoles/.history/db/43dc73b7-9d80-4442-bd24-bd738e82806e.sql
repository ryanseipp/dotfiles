SELECT COUNT(al.Id)
FROM dbo.AuditLog AS al
WHERE (al.OrderId = @orderId AND DAY(al.ModifiedOn) = DAY(@today))
  AND (JSON_VALUE(al.Log, '$.request.Model.({@jsonPath})')) = @jsonValue
;-- -. . -..- - / . -. - .-. -.--
SELECT COUNT(al.Id)
FROM AuditLog AS al
WHERE (al.OrderId = 1 AND DAY(al.ModifiedOn) = DAY('2019-07-11'))
  AND (JSON_VALUE(al.Log, '$.request.Model.CurrentShares')) = 100
;-- -. . -..- - / . -. - .-. -.--
SELECT COUNT(al.Id)
FROM dbo.AuditLog AS al
WHERE (al.OrderId = 1 AND DAY(al.ModifiedOn) = DAY('2019-07-11'))
  AND (JSON_VALUE(al.Log, '$.request.Model.CurrentShares')) = 100